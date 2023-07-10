using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace SvgQrCodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Запуск приложения ===");

            var basedir = "BASEDIR";
            Environment.SetEnvironmentVariable(basedir, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            var config = new Config().Build();

            Console.WriteLine("=== Запуск генерации ===");
            Generate(config);

            Console.WriteLine("=== Генерирование завершено ==="
                + "\r\n"
                + "Для закрытия приложения, нажмите любую кнопку");

            Console.ReadKey();
        }

        private static void Generate(Config config)
        {
            try
            {
                var ouputFolder = $"{Environment.GetEnvironmentVariable("BASEDIR")}\\output";

                if (!Directory.Exists(ouputFolder))
                {
                    Directory.CreateDirectory(ouputFolder);
                }

                var stringFormat = new string('0', config.GenerationPartLength);
                Console.WriteLine($"Маска формата номеров: {stringFormat}");

                for (var value = config.StartNumber; value < config.LastNumber; value += config.StepNumber)
                {
                    var formatedValue = value.ToString(stringFormat);
                    var qrValue = $"{config.FixedPart}{formatedValue}";
                    Console.WriteLine($"QR: {qrValue}");

                    var hints = new Dictionary<EncodeHintType, object>
                    {
                        { EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.M },
                        { EncodeHintType.CHARACTER_SET, "UTF-8" }
                    };

                    var qrCodeWriter = new QRCodeWriter();
                    BitMatrix bitMatrix = qrCodeWriter.encode(qrValue, BarcodeFormat.QR_CODE, config.QrSideSize, config.QrSideSize, hints);

                    StringBuilder sbPath = new StringBuilder();
                    int width = bitMatrix.Width;
                    int height = bitMatrix.Height;
                    int rowSize = bitMatrix.RowSize;

                    BitArray row = new BitArray(width);
                    for (int y = 0; y < height; ++y)
                    {
                        row = bitMatrix.getRow(y, row);
                        for (int x = 0; x < width; ++x)
                        {
                            if (row[x])
                            {
                                sbPath.Append($" M{x},{y}h1v1h-1z");
                            }
                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                    sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" ")
                        .Append($"width=\"{config.QrSideSize}mm\" height=\"{config.QrSideSize}mm\" ")
                        .Append($"viewBox =\"0 0 {config.SvgPixelBoxSize} {config.SvgPixelBoxSize}\" ")
                        .Append("stroke=\"none\">\n");
                    sb.Append("<style type=\"text/css\">\n");
                    sb.Append(".black {fill:#000000;}\n");
                    sb.Append("</style>\n");
                    sb.Append("<path class=\"black\"  d=\"").Append(sbPath.ToString()).Append("\"/>\n");
                    sb.Append("</svg>\n");

                    File.WriteAllText($"{ouputFolder}\\{qrValue}.svg", sb.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при выполнении кода: \r\n{ex}");
            }
        }

    }

    class Config
    {
        public string FixedPart;
        public int GenerationPartLength;
        public int StartNumber;
        public int LastNumber;
        public int StepNumber;
        public int QrSideSize;
        public int SvgPixelBoxSize;

        public Config Build()
        {
            SetFixedPart();
            SetGenerationPartLength();
            SetStartNumber();
            SetLastNumber();
            SetStepNumber();
            SetQrSideSize();
            SetSvgPixelBoxSize();

            return this;
        }

        public Config SetFixedPart()
        {
            Console.Write("Введите фиксированную часть: ");
            FixedPart = Console.ReadLine();
            return this;
        }

        public Config SetGenerationPartLength()
        {
            Console.Write("Введите кол-во чисел в правой части: ");
            GenerationPartLength = 0;
            while (!int.TryParse(Console.ReadLine(), out GenerationPartLength) || GenerationPartLength <= 0)
            {
                ShowNumberErrorMessage();
            }
            return this;
        }

        public Config SetStartNumber()
        {
            Console.Write("Введите стартовое число: ");
            StartNumber = 0;
            while (!int.TryParse(Console.ReadLine(), out StartNumber) || StartNumber < 0)
            {
                ShowNumberErrorMessage();
            }
            return this;
        }

        public Config SetLastNumber()
        {
            Console.Write("Введите конечное число: ");
            LastNumber = 0;
            while (!int.TryParse(Console.ReadLine(), out LastNumber) || LastNumber < 0 || LastNumber <= StartNumber)
            {
                ShowNumberErrorMessage();
            }
            return this;
        }

        public Config SetStepNumber()
        {
            Console.Write("Введите шаг: ");
            StepNumber = 1;
            while (!int.TryParse(Console.ReadLine(), out StepNumber) || StepNumber < 0 || StepNumber >= LastNumber)
            {
                ShowNumberErrorMessage();
            }
            return this;
        }

        public Config SetQrSideSize()
        {
            Console.Write("Введите размер стороны Qr-кода, в миллиметрах (например: 20): ");
            QrSideSize = 0;
            while (!int.TryParse(Console.ReadLine(), out QrSideSize) || QrSideSize <= 0)
            {
                ShowNumberErrorMessage();
            }
            return this;
        }

        public Config SetSvgPixelBoxSize()
        {
            Console.Write("Введите размер Svg объекта в пикселях (например: 21): ");
            SvgPixelBoxSize = 0;
            while (!int.TryParse(Console.ReadLine(), out SvgPixelBoxSize) || SvgPixelBoxSize <= 0)
            {
                ShowNumberErrorMessage();
            }
            return this;
        }

        private static void ShowNumberErrorMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Введите верное число!");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

    }
}
