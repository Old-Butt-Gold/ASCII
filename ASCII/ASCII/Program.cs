namespace ASCII_art_lesson
{
    class Program
    {
        const double WIDTH_OFFSET = 1.7; //1.5
        const int maxWidth = 474; //800
        [STAThread]
        static void Main(string[] args)
        {
            var openFileDialog = new OpenFileDialog() { Filter = "Images | *.bmp; *.png; *.jpg; *.JPEG" };
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.ReadLine();
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    continue;
                Console.Clear();

                var bitmap = ResizeBitmap(new Bitmap(openFileDialog.FileName));
                bitmap.ToGrayScale();
                
                var converter = new BitmapToASCIIConverter(bitmap);
                var rows = converter.Convert();

                foreach (var row in rows)
                    Console.WriteLine(row);
                Console.SetCursorPosition(0,0);

                var saveDialog = new SaveFileDialog() {Filter = "Textfile | *.txt"};
                if (saveDialog.ShowDialog() != DialogResult.OK)
                    continue;
                var rowNegative = converter.ConvertNegative();
                Write();
                async void Write()
                {
                    await File.WriteAllLinesAsync(saveDialog.FileName, rowNegative.Select(x => new string(x)));
                }
            }
        }

        static Bitmap ResizeBitmap(Bitmap bitmap)
        {
            var maxWidth = Program.maxWidth;
            var newHeight = bitmap.Height / WIDTH_OFFSET * maxWidth / bitmap.Width;
            if (bitmap.Width > maxWidth || bitmap.Height > newHeight)
                bitmap = new(bitmap, new Size(maxWidth, (int)newHeight));
            return bitmap;
        }
    }
}

public static class Extensions
{
    public static void ToGrayScale(this Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                int avg = (pixel.R + pixel.G + pixel.B) / 3;
                bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, avg, avg, avg));
            }
    }
}

public class BitmapToASCIIConverter
{
    //На белом фоне поменять в другом порядке
    readonly char[] _asciiTable = { '.', ',', ':', '+', '*', '?', '%', 'S', '#', '@' };
    readonly char[] _asciiTableNegative = { '@', '#', 'S', '%', '?', '*', '+', ':', ',', '.' };
    readonly Bitmap _bitmap;
    public BitmapToASCIIConverter(Bitmap bitmap) => _bitmap = bitmap;

    public char[][] Convert() => Convert(_asciiTable);

    public char[][] ConvertNegative() => Convert(_asciiTableNegative);
    char[][] Convert(char[] asciiTable)
    {
        var result = new char[_bitmap.Height][];
        for (int y = 0; y < _bitmap.Height; y++)
        {
            result[y] = new char[_bitmap.Width];
            for (int x = 0; x < _bitmap.Width; x++)
            {
                int mapIndex = (int)Map(_bitmap.GetPixel(x, y).R, 0, 255, 0, asciiTable.Length - 1);
                result[y][x] = asciiTable[mapIndex];
            }
        }
        return result;
    }

    float Map(float valueToMap, float start1, float stop1, float start2, float stop2)
    {
        return (valueToMap - start1) / (stop1 - start1) * (stop2 - start2) + start2;
    }
}