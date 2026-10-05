using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CanteenBackend.Models;

public class InvoiceDocument : IDocument
{
    private readonly Order _order;

    public InvoiceDocument(Order order)
    {
        _order = order ?? throw new ArgumentNullException(nameof(order));
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);

            page.DefaultTextStyle(x =>
                x.FontFamily("Lato")
                 .FontSize(10)
            );

            // HEADER
            page.Header()
                .Element(ComposeHeader);

            // CONTENT
            page.Content()
                .PaddingTop(20)
                .Column(column =>
                {
                    column.Spacing(20);

                    column.Item()
                        .Element(ComposeCustomerDetails);

                    column.Item()
                        .Element(ComposeItemsTable);

                    column.Item()
                        .Element(ComposeTotals);
                });

            // FOOTER
            page.Footer()
                .AlignCenter()
                .Text("Thank you for your order!")
                .FontSize(16)
                .FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeHeader(IContainer container)
{
    container
        .AlignCenter()
        .Column(column =>
        {
            // Logo
            column.Item()
                .AlignCenter()
                .Width(90)
                .Height(70)
                .Image("Assets/logo.png")
                .FitArea();

            // Line
            column.Item()
                .PaddingTop(20)
                .Width(300)
                .LineHorizontal(2);

            // INVOICE
            column.Item()
                .PaddingTop(15)
                .AlignCenter()
                .Text("INVOICE")
                .FontSize(24)
                .Bold();

            // Invoice Number
            column.Item()
                .PaddingTop(5)
                .AlignCenter()
                .Text($"#{_order.Id:D4}")
                .FontSize(11);

            // Date
            column.Item()
                .PaddingTop(3)
                .AlignCenter()
                .Text(_order.CreatedAt.ToString("dd MMM yyyy"))
                .FontSize(9)
                .FontColor(Colors.Grey.Darken1);
        });
}
    private void ComposeCustomerDetails(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderBottom(1)
            .PaddingVertical(12)
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("BILL TO")
                            .Bold()
                            .FontSize(9);

                        column.Item()
                            .PaddingTop(4)
                            .Text(_order.User?.FullName ?? "Customer");

                        column.Item()
                            .Text(_order.User?.MobileNumber ?? "");
                    });

                row.RelativeItem()
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("ORDER TYPE")
                            .Bold()
                            .FontSize(9);

                        column.Item()
                            .PaddingTop(4)
                            .Text(GetOrderType(_order.OrderType));
                    });
            });
    }

    private void ComposeItemsTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(8)
                    .Text("ITEM")
                    .Bold();

                header.Cell()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(8)
                    .AlignCenter()
                    .Text("QTY")
                    .Bold();

                header.Cell()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(8)
                    .AlignRight()
                    .Text("PRICE")
                    .Bold();

                header.Cell()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(8)
                    .AlignRight()
                    .Text("TOTAL")
                    .Bold();
            });

            foreach (var item in _order.Items)
            {
                table.Cell()
                    .PaddingVertical(8)
                    .Text(item.Item?.Name ?? "Unknown Item");

                table.Cell()
                    .PaddingVertical(8)
                    .AlignCenter()
                    .Text(item.Quantity.ToString());

                table.Cell()
                    .PaddingVertical(8)
                    .AlignRight()
                    .Text($"₹ {item.Price:N2}");

                table.Cell()
                    .PaddingVertical(8)
                    .AlignRight()
                    .Text($"₹ {(item.Quantity * item.Price):N2}");
            }
        });
    }

    private void ComposeTotals(IContainer container)
    {
        container.AlignRight().Width(220).Column(column =>
        {
            column.Item()
                .Row(row =>
                {
                    row.RelativeItem().Text("Subtotal");
                    row.ConstantItem(90)
                        .AlignRight()
                        .Text($"₹ {_order.SubTotal:N2}");
                });

            column.Item()
                .Row(row =>
                {
                    row.RelativeItem().Text("Discount");
                    row.ConstantItem(90)
                        .AlignRight()
                        .Text($"- ₹ {_order.Discount:N2}");
                });

            column.Item()
                .PaddingTop(8)
                .BorderTop(1)
                .PaddingTop(8)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("TOTAL")
                        .Bold()
                        .FontSize(13);

                    row.ConstantItem(90)
                        .AlignRight()
                        .Text($"₹ {_order.FinalAmount:N2}")
                        .Bold()
                        .FontSize(13);
                });
        });
    }

    private string GetOrderType(int orderType)
    {
        return orderType switch
        {
            1 => "Dine-In",
            2 => "Takeaway",
            _ => "Order"
        };
    }
}