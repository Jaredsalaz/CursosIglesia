using CursosIglesiaAPI.Models.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CursosIglesiaAPI.Services.Implementations
{
    public class CertificatePdfGenerator
    {
        public static byte[] GeneratePdf(CertificateListDto certificate)
        {
            // QuestPDF License (Community is free for small/medium usage)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    // Background crema (#FEFAF3)
                    page.PageColor("#FEFAF3");

                    page.Background().Padding(20).Border(5).BorderColor("#C5A059").Padding(5).Border(1).BorderColor("#C5A059");

                    page.Header().Element(Header);
                    page.Content().Element(Content);
                    page.Footer().Element(Footer);

                    void Header(IContainer c)
                    {
                        c.AlignCenter().PaddingTop(30).Column(column =>
                        {
                            column.Item().Text("Cursos Católicos").FontFamily(Fonts.TimesNewRoman).FontSize(24).FontColor("#C5A059").SemiBold();
                            column.Item().Text("Certificado de Finalización").FontFamily(Fonts.TimesNewRoman).FontSize(36).FontColor("#333333").Bold().AlignCenter();
                        });
                    }

                    void Content(IContainer c)
                    {
                        c.PaddingVertical(40).Column(column =>
                        {
                            column.Item().AlignCenter().Text("Se otorga el presente certificado a:").FontSize(16).FontColor("#666666").Italic();
                            column.Item().PaddingVertical(15).AlignCenter().Text(certificate.NombreEstudiante).FontFamily(Fonts.TimesNewRoman).FontSize(42).Bold().FontColor("#111111");
                            column.Item().AlignCenter().Text("Por haber completado satisfactoriamente los requisitos del curso:").FontSize(16).FontColor("#666666").Italic();
                            column.Item().PaddingVertical(15).AlignCenter().Text(certificate.NombreCurso.ToUpper()).FontFamily(Fonts.TimesNewRoman).FontSize(28).SemiBold().FontColor("#C5A059");

                            column.Item().PaddingTop(30).PaddingHorizontal(50).Row(row =>
                            {
                                row.RelativeItem().AlignCenter().Column(col =>
                                {
                                    col.Item().LineHorizontal(1).LineColor("#333333");
                                    col.Item().PaddingTop(5).Text(certificate.NombreInstructor).FontSize(14).Bold();
                                    col.Item().Text("Instructor del Curso").FontSize(12).FontColor("#666666").Italic();
                                });

                                row.RelativeItem().PaddingHorizontal(20);

                                row.RelativeItem().AlignCenter().Column(col =>
                                {
                                    col.Item().LineHorizontal(1).LineColor("#333333");
                                    col.Item().PaddingTop(5).Text(certificate.FechaOtorgamiento.ToString("dd 'de' MMMM, yyyy")).FontSize(14).Bold();
                                    col.Item().Text("Fecha de Emisión").FontSize(12).FontColor("#666666").Italic();
                                });
                            });
                        });
                    }

                    void Footer(IContainer c)
                    {
                        c.PaddingBottom(20).PaddingHorizontal(20).Row(row =>
                        {
                            // QR Code
                            if (!string.IsNullOrEmpty(certificate.CodigoQR))
                            {
                                try
                                {
                                    var qrBytes = Convert.FromBase64String(certificate.CodigoQR);
                                    row.ConstantItem(80).Height(80).Image(qrBytes);
                                }
                                catch
                                {
                                    row.ConstantItem(80); // spacer if invalid base64
                                }
                            }
                            else
                            {
                                row.ConstantItem(80);
                            }

                            row.RelativeItem().AlignCenter().AlignBottom().Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(10).FontColor("#999999"));
                                text.Line($"ID de Certificado: {certificate.NumeroCertificado}");
                                text.Line("Para verificar la autenticidad de este documento, escanee el código QR o visite la plataforma.");
                            });

                            row.ConstantItem(80).Height(80).AlignCenter().AlignMiddle().Text("SELLO").FontColor("#E5D0B1").FontSize(20).Bold();
                        });
                    }
                });
            });

            return document.GeneratePdf();
        }
    }
}
