
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
    const double holeCX = 13.8; // 3mm bit overcuts 0.1mm, result is 14.0
    const double holeCY = 13.5; // result is 13.7
    const double thinnerCY = 1;

    // фреза
    const double millD = 3;
    const double millR = millD / 2;

    const double safeZ = 5;
    const double slowZ = 1;
    const double plateTopZ = 0;
    const double proposedStepDown = 0.3;
    const double finalCutDepthZ = (plateTopZ - 2.1);
    const double thinnerDepthZ = plateTopZ - 0.5;
    const double plateBorder = 5;
    const double backWallCY = 22;
    const double frontWallCY = 12;

    // SVG image size in mm  
    const double svgWidth = u1 * 18.5 + plateBorder * 2;
    const double svgHeight = u1 * 6.5 + plateBorder * 2 + frontWallCY + backWallCY;

    bool writeGCode = false;
    bool writeSVG => !writeGCode;


    Program()
    {
      Comment($"Program starts");

      //CutU1(0, 0);
      CutAllKeys();

      Comment("Program stops");
      JogZ(safeZ);
      JogXY(0, 0);
    }

    void CutAllKeys()
    {
      WriteSVGStart();

      double rowX = plateBorder;
      double rowY = frontWallCY + plateBorder + u1 * 5.5;

      double plateCX = u1 * 18.5 + plateBorder * 2;
      double plateCY = u1 * 6.5 + plateBorder * 2;
      DrawSVGRect(0, 0, plateCX, frontWallCY); 
      DrawSVGRect(0, frontWallCY, plateCX, frontWallCY + plateCY);
      DrawSVGRect(0, frontWallCY + plateCY, plateCX, frontWallCY + plateCY + backWallCY);

      // 1st row: Escape
      CutUnit(rowX, rowY, 1);
      for (int i = 0; i < 4; i++) CutU1(rowX + u1 * 2 + i * u1, rowY);
      for (int i = 0; i < 4; i++) CutU1(rowX + u1 * 6.5 + i * u1, rowY);
      for (int i = 0; i < 4; i++) CutU1(rowX + u1 * 11 + i * u1, rowY);
      for (int i = 0; i < 3; i++) CutU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      // 2nd row: digits
      rowY -= u1 * 1.5;
      for (int i = 0; i < 13; i++) CutU1(rowX + i * u1, rowY);
      CutUnit(rowX + 13 * u1, rowY, 2);
      for (int i = 0; i < 3; i++) CutU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      // 3rd row: Tab
      rowY -= u1;
      CutUnit(rowX, rowY, 1.5); 
      for (int i = 0; i < 12; i++) CutU1(rowX + u1 * 1.5 + i * u1, rowY);
      CutUnit(rowX + u1 * 1.5 + 12 * u1, rowY, 1.5);
      for (int i = 0; i < 3; i++) CutU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      // 4th row: Caps Lock
      rowY -= u1;
      CutUnit(rowX, rowY, 1.75); 
      for (int i = 0; i < 11; i++) CutU1(rowX + u1 * 1.75 + i * u1, rowY);
      CutUnit(rowX + u1 * 1.75 + 11 * u1, rowY, 2.25); // enter

      // 5th row: Shift
      rowY -= u1;
      CutUnit(rowX, rowY, 2.25);
      for (int i = 0; i < 10; i++) CutU1(rowX + u1 * 2.25 + i * u1, rowY);
      CutUnit(rowX + u1 * 2.25 + 10 * u1, rowY, 2.75); // right shift
      CutU1(rowX + u1 * 16.5, rowY); // arrow up

      // 6th row: Space
      rowY -= u1;
      for(int i = 0; i < 3; i++) CutUnit(rowX + i * 1.25 * u1, rowY, 1.25);
      CutUnit(rowX + u1 * 3.75, rowY, 6.25); // Space
      for (int i = 0; i < 4; i++) CutUnit(rowX + u1 * (3.75 + 6.25) + i * 1.25 * u1, rowY, 1.25);
      for (int i = 0; i < 3; i++) CutU1(rowX + u1 * 15.5 + i * u1, rowY); // arrows

      WriteSVGStop();
    }

    void CutU1(double unitX, double unitY) => CutUnit(unitX, unitY, 1);

    void CutUnit(double unitX, double unitY, double widthInUnits)
    {
      // границы отверстия
      double holeX1 = unitX + ((u1 * widthInUnits) - holeCX) / 2;
      double holeX2 = holeX1 + holeCX;
      double holeY1 = unitY + (u1 - holeCY) / 2;
      double holeY2 = holeY1 + holeCY;

      DrawSVGRect(unitX, unitY, unitX + u1 * widthInUnits, unitY + u1);
      DrawSVGRect(holeX1, holeY1, holeX2, holeY2);

      JogZ(safeZ);
      Comment("Unit pocket");
      CutSquare(true, holeX1, holeY1 - thinnerCY, holeX2, holeY2 + thinnerCY, thinnerDepthZ);

      double topZ = thinnerDepthZ;
      double bottomZ = finalCutDepthZ;
      int numSteps = (int)((topZ - bottomZ) / proposedStepDown);
      double realStepZ = (topZ - bottomZ) / numSteps;
      for (int i = 0; i < numSteps; i++) {
        Comment($"Cut {i + 1}/{numSteps} zDown={D2S(realStepZ)}");
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

    void JogZ(double z) => WriteGCodeLine($"G0 Z{D2S(z)}");

    void JogXY(double x, double y) => WriteGCodeLine($"G0 X{D2S(x)} Y{D2S(y)}");

    void Cut(double x, double y, double z) => CutF(x, y, z, cuttingSpeedF);

    void CutF(double x, double y, double z, string speedF) 
      => WriteGCodeLine($"G1 X{D2S(x)} Y{D2S(y)} Z{D2S(z)} {speedF}");

    void Comment(string s) => WriteGCodeLine($"({s})");

    void WriteGCodeLine(string s)
    {
      if (writeGCode) Console.WriteLine(s);
    }

    readonly NumberFormatInfo dotNFI = 
      new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };

    string D2S(double d) => string.Format(dotNFI, "{0:N}", d);

    void DrawSVGRect(double x1, double y1, double x2, double y2)
    {
      // SVG Y-coordinate goes top-down, GCode Y-coordinate goes down-top
      WriteSVGLine(
        $"<rect x='{D2S(x1)}' y='{D2S(svgHeight - y2)}' width='{D2S(x2 - x1)}'" +
        $" height='{D2S(y2 - y1)}' stroke='gray' fill='none'/>");
    }
    void WriteSVGStart()
    {
      WriteSVGLine($"<svg xmlns='http://www.w3.org/2000/svg'");
      WriteSVGLine($" width='{D2S(svgWidth)}mm' height='{D2S(svgHeight)}mm'");
      WriteSVGLine($" viewBox='0 0 {D2S(svgWidth)} {D2S(svgHeight)}'>");
    }

    void WriteSVGStop() => WriteSVGLine("</svg>");

    void WriteSVGLine(string s)
    {
      if (writeSVG) Console.WriteLine(s);
    }    
  }
}
