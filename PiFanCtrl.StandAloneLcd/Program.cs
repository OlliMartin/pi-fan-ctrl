using System.Device.I2c;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;

try
{
  SkiaSharpAdapter.Register();

  Console.WriteLine("Using direct I2C protocol");

  I2cConnectionSettings connectionSettings = new(busId: 1, deviceAddress: 0x3C);
  using I2cDevice? i2cDevice = I2cDevice.Create(connectionSettings);
  using Ssd1306 device = new(i2cDevice, width: 128, height: 32);

  device.ClearScreen();

  Console.WriteLine("Display clock");
  int fontSize = 25;
  string font = "DejaVu Sans";

  while (!Console.KeyAvailable)
    using (BitmapImage image = BitmapImage.CreateBitmap(width: 128, height: 32, PixelFormat.Format32bppArgb))
    {
      int y = 0;
      image.Clear(Color.Black);

      IGraphics g = image.GetDrawingApi();

      g.DrawText(DateTime.Now.ToString("HH:mm:ss"), font, fontSize, Color.White, new(x: 0, y));

      device.DrawBitmap(image);

      Thread.Sleep(millisecondsTimeout: 100);
    }

  Console.ReadKey(intercept: true);
}
catch (Exception ex)
{
  Console.WriteLine("An error occurred:");
  Console.WriteLine(ex);
}