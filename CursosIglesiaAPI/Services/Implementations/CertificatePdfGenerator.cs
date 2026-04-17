using CursosIglesiaAPI.Models.DTOs;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using System.IO;

namespace CursosIglesiaAPI.Services.Implementations;

public static class CertificatePdfGenerator
{
    // Colores pergamino clásico
    private static readonly BaseColor ColorSepia = new BaseColor(112, 77, 20);        // Sepia
    private static readonly BaseColor ColorOro = new BaseColor(212, 175, 55);          // Oro
    private static readonly BaseColor ColorCrema = new BaseColor(254, 250, 243);       // Crema
    private static readonly BaseColor ColorTexto = new BaseColor(70, 50, 20);          // Marrón oscuro

    /// <summary>
    /// Genera un PDF profesional de certificado
    /// </summary>
    public static byte[] GenerateCertificatePdf(CertificateResponse certificate)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // A4 horizontal (297mm x 210mm -> 842 x 595 puntos)
            Rectangle pageSize = new Rectangle(842, 595);
            Document document = new Document(pageSize, 40, 40, 40, 40);
            PdfWriter writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // 1. Fondo pergamino (color crema)
            PdfContentByte cb = writer.DirectContent;
            cb.SetColorFill(ColorCrema);
            cb.Rectangle(0, 0, pageSize.Width, pageSize.Height);
            cb.Fill();

            // 2. Bordes ornamentales (líneas doradas)
            cb.SetLineWidth(3f);
            cb.SetColorStroke(ColorOro);
            cb.Rectangle(20, 20, pageSize.Width - 40, pageSize.Height - 40);
            cb.Stroke();

            // Línea interior más delgada
            cb.SetLineWidth(1f);
            cb.SetColorStroke(ColorSepia);
            cb.Rectangle(30, 30, pageSize.Width - 60, pageSize.Height - 60);
            cb.Stroke();

            // 3. Título principal
            Font titleFont = new Font(Font.FontFamily.TIMES_ROMAN, 42, Font.BOLD, ColorSepia);
            Paragraph title = new Paragraph("CERTIFICADO", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5f
            };
            document.Add(title);

            Font subtitleFont = new Font(Font.FontFamily.TIMES_ROMAN, 18, Font.BOLD, ColorOro);
            Paragraph subtitle = new Paragraph("DE FINALIZACIÓN", subtitleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30f
            };
            document.Add(subtitle);

            // 4. Texto "SE CERTIFICA QUE:"
            Font headerFont = new Font(Font.FontFamily.TIMES_ROMAN, 14, Font.NORMAL, ColorTexto);
            Paragraph certifies = new Paragraph("SE CERTIFICA QUE:", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(certifies);

            // 5. Nombre del estudiante (con subrayado)
            Font studentNameFont = new Font(Font.FontFamily.TIMES_ROMAN, 28, Font.BOLD, ColorSepia);
            Paragraph studentName = new Paragraph(certificate.NombreEstudiante.ToUpper(), studentNameFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5f
            };
            document.Add(studentName);

            // Línea decorativa bajo nombre
            Paragraph nameUnderline = new Paragraph("═════════════════════════════════════", new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.NORMAL, ColorOro))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(nameUnderline);

            // 6. Descripción
            Font descriptionFont = new Font(Font.FontFamily.TIMES_ROMAN, 13, Font.NORMAL, ColorTexto);
            Paragraph description = new Paragraph("Ha completado satisfactoriamente el curso:", descriptionFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            document.Add(description);

            // 7. Nombre del curso
            Font courseTitleFont = new Font(Font.FontFamily.TIMES_ROMAN, 24, Font.BOLD, ColorOro);
            Paragraph courseTitle = new Paragraph(certificate.NombreCurso, courseTitleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5f
            };
            document.Add(courseTitle);

            // Línea bajo curso
            Paragraph courseUnderline = new Paragraph("═════════════════════════════════════", new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.NORMAL, ColorOro))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(courseUnderline);

            // 8. Instructor
            Font instructorLabelFont = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.NORMAL, ColorTexto);
            Paragraph instructorLabel = new Paragraph("Bajo la supervisión de:", instructorLabelFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5f
            };
            document.Add(instructorLabel);

            Font instructorNameFont = new Font(Font.FontFamily.TIMES_ROMAN, 16, Font.BOLD, ColorSepia);
            Paragraph instructorName = new Paragraph(certificate.NombreInstructor ?? "Instructor", instructorNameFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 25f
            };
            document.Add(instructorName);

            // 9. Fecha y número de certificado
            Font dateFont = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL, ColorTexto);
            string formatDate = certificate.FechaOtorgamiento.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            Paragraph dateInfo = new Paragraph($"Emitido en: {formatDate}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5f
            };
            document.Add(dateInfo);

            Paragraph certNumber = new Paragraph($"Certificado Nº: {certificate.NumeroCertificado}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 25f
            };
            document.Add(certNumber);

            // 10. Líneas de firma (tabla con 3 columnas)
            PdfPTable signaturesTable = new PdfPTable(3)
            {
                WidthPercentage = 80f,
                SpacingBefore = 20f,
                SpacingAfter = 15f
            };

            // Células sin bordes
            foreach (PdfPCell cell in GetSignatureCells())
            {
                signaturesTable.AddCell(cell);
            }

            document.Add(signaturesTable);

            // 11. QR Code (esquina inferior derecha)
            try
            {
                string qrContent = $"https://iglesia.com/verify/{certificate.NumeroCertificado}";
                byte[] qrImage = GenerateQRCode(qrContent);

                Image qr = Image.GetInstance(qrImage);
                qr.ScaleToFit(80f, 80f);

                // Posicionar en esquina inferior derecha
                PdfPTable qrTable = new PdfPTable(1)
                {
                    WidthPercentage = 100f
                };
                PdfPCell qrCell = new PdfPCell(qr) { Border = PdfPCell.NO_BORDER };
                qrCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                qrTable.AddCell(qrCell);
                document.Add(qrTable);
            }
            catch
            {
                // Si falla QR, continuar sin él
            }

            // 12. Pie (texto oficial)
            Font footerFont = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.NORMAL, ColorOro);
            Paragraph footer = new Paragraph("═ CERTIFICADO OFICIAL - PLATAFORMA DE FORMACIÓN CATÓLICA ═", footerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 15f
            };
            document.Add(footer);

            document.Close();
            return ms.ToArray();
        }
    }

    private static List<PdfPCell> GetSignatureCells()
    {
        Font signFont = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.NORMAL, ColorTexto);

        var cells = new List<PdfPCell>();

        // Columna 1: Firma Autoridad
        PdfPCell cell1 = new PdfPCell(new Phrase("_________________\nFirma Autoridad", signFont))
        {
            Border = PdfPCell.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER,
            PaddingBottom = 30f
        };
        cells.Add(cell1);

        // Columna 2: Sello (espaciada)
        PdfPCell cell2 = new PdfPCell(new Phrase("", signFont))
        {
            Border = PdfPCell.NO_BORDER
        };
        cells.Add(cell2);

        // Columna 3: Firma Profesor
        PdfPCell cell3 = new PdfPCell(new Phrase("_________________\nFirma Profesor", signFont))
        {
            Border = PdfPCell.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER,
            PaddingBottom = 30f
        };
        cells.Add(cell3);

        return cells;
    }

    private static byte[] GenerateQRCode(string content)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                using (Bitmap qrImage = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
