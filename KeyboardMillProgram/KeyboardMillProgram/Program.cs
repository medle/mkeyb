
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMillProgram
{
  class Program
  {
    static void Main(string[] args)
    {
      new Program();
    }

    const string cuttingSpeedF = "F60";
    const string drillSpeedF = "F10";

    // размеры блока кнопки 1U
    const double inch = 25.4;
    const double u1 = inch * 0.75;

    // размеры отверстия
    const double holeCX = 14;
    const double holeCY = 13.5;
    const double thinCY = 0.5;

    // фреза
    const double millD = 3;
    const double millR = millD / 2;

    const double safeZ = 5;
    const double slowZ = 1;
    const double plateTopZ = 0;

    Program()
    {
      Comment($"HoleCX={D2S(holeCX)} HoleCY={D2S(holeCY)}");
      Comment($"Program starts");

      CutU1(0, 0);

      Comment("Program stops");
      JogZ(safeZ);
      JogXY(0, 0);
    }

    void CutU1(double unitX, double unitY)
    {
      // границы отверстия
      double holeX1 = unitX + (u1 - holeCX) / 2;
      double holeX2 = holeX1 + holeCX;
      double holeY1 = unitY + (u1 - holeCY) / 2;
      double holeY2 = holeY1 + holeCY;

      JogZ(safeZ);
      Comment("Pocket");
      CutSquare(true, holeX1, holeY1 - thinCY, holeX2, holeY2 + thinCY, plateTopZ - 0.5);

      double topZ = plateTopZ - 0.5;
      double bottomZ = plateTopZ - 2.5;
      double proposedStepZ = 0.3;
      int numSteps = (int)((topZ - bottomZ) / proposedStepZ);
      double realStepZ = (topZ - bottomZ) / numSteps;
      for (int i = 0; i < numSteps; i++) {
        Comment($"Cut {i + 1}/{numSteps} stepDown={D2S(realStepZ)}");
        double cutZ = topZ - (i + 1) * realStepZ;
        CutSquare(false, holeX1, holeY1, holeX2, holeY2, cutZ);
      }

      double off = millR * (1 - Math.Cos(Math.PI / 4));
      Comment($"Corner00 off={off}");
      CutSafeHole(holeX1 - off + millR, holeY1 - off + millR, plateTopZ - 2.5);
      Comment("Corner01");
      CutSafeHole(holeX1 - off + millR, holeY2 + off - millR, plateTopZ - 2.5);
      Comment("Corner11");
      CutSafeHole(holeX2 + off - millR, holeY2 + off - millR, plateTopZ - 2.5);
      Comment("Corner10");
      CutSafeHole(holeX2 + off - millR, holeY1 - off + millR, plateTopZ - 2.5);
    }

    void CutSafeHole(double x, double y, double finalZ)
    {
      JogZ(safeZ);
      JogXY(x, y);
      JogZ(slowZ);
      CutF(x, y, finalZ, drillSpeedF);
      JogZ(safeZ);
    }

    void CutSquare(bool jogToStart, double x1, double y1, double x2, double y2, double z)
    {
      double millX1 = x1 + millR;
      double millY1 = y1 + millR;
      double millX2 = x2 - millR;
      double millY2 = y2 - millR;

      if (jogToStart) {
        JogZ(safeZ);
        JogXY(millX1, millY1);
        JogZ(slowZ);
      }

      CutF(millX1, millY1, z, drillSpeedF);
      Cut(millX1, millY1, z); // inside
      Cut(millX1, millY2, z); // up
      Cut(millX2, millY2, z); // right
      Cut(millX2, millY1, z); // down
      Cut(millX1, millY1, z); // left
    }

    void JogZ(double z) => WriteLine($"G0 Z{D2S(z)}");

    void JogXY(double x, double y) => WriteLine($"G0 X{D2S(x)} Y{D2S(y)}");

    void Cut(double x, double y, double z) => CutF(x, y, z, cuttingSpeedF);

    void CutF(double x, double y, double z, string speedF) 
      => WriteLine($"G1 X{D2S(x)} Y{D2S(y)} Z{D2S(z)} {speedF}");

    void Comment(string s) => WriteLine($"({s})");

    void WriteLine(string s) => Console.WriteLine(s);

    readonly NumberFormatInfo dotNFI = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };

    string D2S(double d) => string.Format(dotNFI, "{0:N}", d);
  }
}
