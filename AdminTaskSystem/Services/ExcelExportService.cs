using System.Diagnostics;
using AdminTaskSystem.Models;
using ClosedXML.Excel;

namespace AdminTaskSystem.Services;

public sealed class ExcelExportService
{
    public void ExportTickets(IEnumerable<TicketFull> tickets, string filePath)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Заявки");
        var headers = new[] { "№", "Тема", "Категория", "Заявитель", "Контакт", "Отдел", "Исполнитель", "Статус", "Дедлайн", "Создана" };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
        }

        var row = 2;
        foreach (var ticket in tickets)
        {
            sheet.Cell(row, 1).Value = ticket.Id;
            sheet.Cell(row, 2).Value = ticket.Title;
            sheet.Cell(row, 3).Value = ticket.CategoryName;
            sheet.Cell(row, 4).Value = ticket.ApplicantName;
            sheet.Cell(row, 5).Value = ticket.ApplicantContact;
            sheet.Cell(row, 6).Value = ticket.DepartmentName;
            sheet.Cell(row, 7).Value = ticket.AssignedEmployeeName;
            sheet.Cell(row, 8).Value = ToRussianStatus(ticket.Status);
            sheet.Cell(row, 9).Value = ticket.Deadline;
            sheet.Cell(row, 10).Value = ticket.CreatedAt;
            sheet.Cell(row, 9).Style.DateFormat.Format = "dd.MM.yyyy";
            sheet.Cell(row, 10).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";

            var fill = ticket.Status switch
            {
                "overdue" => "#FFCDD2",
                "in_progress" => "#FFF9C4",
                "done" => "#C8E6C9",
                _ => "#FFFFFF"
            };
            sheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml(fill);
            row++;
        }

        sheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }

    public void ExportReports(
        IEnumerable<DepartmentStats> departmentStats,
        IEnumerable<EmployeeStats> employeeStats,
        string filePath)
    {
        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Сводка");
        var departments = departmentStats.ToList();
        var employees = employeeStats.ToList();

        summary.Cell(1, 1).Value = "Показатель";
        summary.Cell(1, 2).Value = "Значение";
        StyleHeader(summary.Range(1, 1, 1, 2));
        summary.Cell(2, 1).Value = "Всего заявок";
        summary.Cell(2, 2).Value = departments.Sum(x => x.Total);
        summary.Cell(3, 1).Value = "Выполнено";
        summary.Cell(3, 2).Value = departments.Sum(x => x.DoneCount);
        summary.Cell(4, 1).Value = "Просрочено";
        summary.Cell(4, 2).Value = departments.Sum(x => x.OverdueCount);
        summary.Cell(5, 1).Value = "Среднее время закрытия, ч";
        summary.Cell(5, 2).Value = departments.Where(x => x.AvgCloseHours.HasValue)
            .Select(x => x.AvgCloseHours!.Value)
            .DefaultIfEmpty(0)
            .Average();
        summary.RangeUsed()?.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summary.RangeUsed()?.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        summary.Columns().AdjustToContents();

        var deptSheet = workbook.Worksheets.Add("По отделам");
        var deptHeaders = new[] { "Отдел", "Всего", "Новые", "В работе", "Выполнено", "Просрочено", "Ср. время (ч)" };
        for (var i = 0; i < deptHeaders.Length; i++)
        {
            deptSheet.Cell(1, i + 1).Value = deptHeaders[i];
        }
        StyleHeader(deptSheet.Range(1, 1, 1, deptHeaders.Length));

        var row = 2;
        foreach (var item in departments)
        {
            deptSheet.Cell(row, 1).Value = item.Department;
            deptSheet.Cell(row, 2).Value = item.Total;
            deptSheet.Cell(row, 3).Value = item.NewCount;
            deptSheet.Cell(row, 4).Value = item.InProgressCount;
            deptSheet.Cell(row, 5).Value = item.DoneCount;
            deptSheet.Cell(row, 6).Value = item.OverdueCount;
            deptSheet.Cell(row, 7).Value = item.AvgCloseHours;
            if (item.OverdueCount > 0)
            {
                deptSheet.Range(row, 1, row, deptHeaders.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFEBEE");
            }
            row++;
        }
        deptSheet.Columns().AdjustToContents();

        var empSheet = workbook.Worksheets.Add("По сотрудникам");
        var empHeaders = new[] { "ФИО", "Отдел", "Заявок всего", "Выполнено", "Просрочено", "Задач всего", "Задач выполнено" };
        for (var i = 0; i < empHeaders.Length; i++)
        {
            empSheet.Cell(1, i + 1).Value = empHeaders[i];
        }
        StyleHeader(empSheet.Range(1, 1, 1, empHeaders.Length));

        row = 2;
        foreach (var item in employees)
        {
            empSheet.Cell(row, 1).Value = item.FullName;
            empSheet.Cell(row, 2).Value = item.Department;
            empSheet.Cell(row, 3).Value = item.TicketsTotal;
            empSheet.Cell(row, 4).Value = item.TicketsDone;
            empSheet.Cell(row, 5).Value = item.TicketsOverdue;
            empSheet.Cell(row, 6).Value = item.TasksTotal;
            empSheet.Cell(row, 7).Value = item.TasksDone;
            if (item.TicketsOverdue > 0)
            {
                empSheet.Range(row, 1, row, empHeaders.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFEBEE");
            }
            row++;
        }
        empSheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
    }

    private static string ToRussianStatus(string status) => status switch
    {
        "new" => "Новая",
        "in_progress" => "В работе",
        "done" => "Выполнена",
        "overdue" => "Просрочена",
        "rejected" => "Отклонена",
        _ => status
    };
}
