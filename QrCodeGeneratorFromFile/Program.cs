using Microsoft.Extensions.Configuration;
using Net.Codecrete.QrCodeGenerator;
using QRCoder;
using Svg;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static QRCoder.SvgQRCode;

namespace QrCodeGeneratorFromFile
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        private static Config _config;

        static void Main(string[] args)
        {

            Console.WriteLine("=== App start ===");

            SetupConfig();
            ReadConfig();
            Generate();
        }

        private static void SetupConfig()
        {
            var basedir = "BASEDIR";
            Environment.SetEnvironmentVariable(basedir, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            var basePath = Environment.GetEnvironmentVariable(basedir);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", false, true);

            _configuration = configurationBuilder.Build();
        }

        private static void ReadConfig()
        {
            Console.WriteLine("Считывание конфига");

            _config = _configuration.GetSection(nameof(Config)).Get<Config>();

            if (string.IsNullOrEmpty(_config.Filename))
            {
                Console.WriteLine("Имя файла пустое");
            }
        }


        private static void Generate()
        {
            try
            {
                var ouputFolder = $"{Environment.GetEnvironmentVariable("BASEDIR")}\\output";

                if (!Directory.Exists(ouputFolder))
                {
                    Directory.CreateDirectory(ouputFolder);
                }

                var stringFormat = new string('0', _config.GenerationPartLength);
                Console.WriteLine($"Маска формата номеров: {stringFormat}");

                QRCodeGenerator qrGenerator = new QRCodeGenerator();

                foreach (int value in Enumerable.Range(_config.StartNumber, _config.LastNumber))
                {
                    var formatedValue = value.ToString(stringFormat);
                    var qrValue = $"{_config.FixedPart}{formatedValue}";
                    Console.WriteLine($"QR: {qrValue}");

                    GeneratePng(qrValue);

                    //QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrValue, QRCodeGenerator.ECCLevel.Q);
                    //SvgQRCode qrCode = new SvgQRCode(qrCodeData);
                    ////var content = qrCode.GetGraphic(20);

                    //var content = qrCode.GetGraphic(
                    //    10, Color.Black, Color.Transparent, false,
                    //    SizingMode.WidthHeightAttribute, null);

                    //File.WriteAllText($"{ouputFolder}\\{formatedValue}.svg", content, Encoding.UTF8);



                    //var content = qrCode.GetGraphic(
                    //    viewBox: new Size(75, 75),
                    //    drawQuietZones: false,
                    //    sizingMode: SizingMode.WidthHeightAttribute,
                    //    logo: null);


                    //var svgDoc = SvgDocument.Open<SvgDocument>($"{ouputFolder}\\{formatedValue}.svg", null);

                    //using (var bitmap = svgDoc.Draw(500, 0))
                    //{
                    //    bitmap.Save($"{ouputFolder}\\{formatedValue}.svg");
                    //}

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private static void GeneratePng(string qrValue)
        {
            var ouputFolder = $"{Environment.GetEnvironmentVariable("BASEDIR")}\\output";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();

            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrValue, QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            //byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

            //var blackColor = Color.FromArgb(0, 0, 0);

            //var black = new byte[] { blackColor.R, blackColor.G, blackColor.B, blackColor.A, };
            //var white = new byte[] { Color.White.R, Color.White.G, Color.White.B, Color.White.A, };

            //byte[] content = qrCode.GetGraphic(10, black, white, true);
            byte[] content = qrCode.GetGraphic(_config.QrSideSize, true);

            Bitmap image;
            using (var stream = new MemoryStream(content))
            {
                image = new Bitmap(stream);
            }

            image.MakeTransparent();

            //var ms = new MemoryStream(qrCodeAsPngByteArr);
            //Image image = Image.FromStream(ms);

            image.Save($"{ouputFolder}\\{qrValue}.wmf", ImageFormat.Wmf);
        }

        private void OldQr(string qrValue)
        {
            //var qr = new QrCode(3, QrCode.Ecc.High, Encoding.ASCII.GetBytes(qrValue));

            //qr = QrCode.EncodeBinary(Encoding.ASCII.GetBytes(qrValue), QrCode.Ecc.Medium);

            //var qr = QrCode.EncodeText(qrValue, QrCode.Ecc.Low);

            //string svg = qr.ToSvgString(0);

            //File.WriteAllText($"{ouputFolder}\\{formatedValue}.svg", svg, Encoding.UTF8);
        }
    }
}
