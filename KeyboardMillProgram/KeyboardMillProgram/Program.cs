
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

    // mill bit
    const double millD = 3;
    const double millR = millD / 2;
    const double millOvercut = 0.1; // 3mm bit overcuts 0.1mm

    // cutting parameters
    const double safeZ = 5;
    const double slowZ = 1;
    const double plateTopZ = 0;
    const double proposedStepDown = 0.3;
    const double finalCutDepthZ = (plateTopZ - 2.1);
    const string cuttingSpeedF = "F60";
    const string drillSpeedF = "F10";

    // key block size 1U
    const double inch = 25.4;
    const double u1 = inch * 0.75;

    // cherry MX switch cutout 
    const double switchHoleCX = 14.0 - (millOvercut * 2); 
    const double switchHoleCY = 13.7 - (millOvercut * 2); 
    const double switchThinnerCY = 1;
    const double switchThinnerDepthZ = plateTopZ - 0.5;

    // panda stabilizer cutout
    const double stabilHoleCX = 6.6 - (millOvercut * 2);
    const double stabilHoleCY = 12.5 - (millOvercut * 2);
    const double stabilCenterCY = 7 - millOvercut;
    const double stabilBulgeBottomCY = stabilCenterCY;
    const double stabilBulgeCX = 0.5;
    const double stabilBulgeCY = 2;

    // keyboard plate sizes
    const double plateBorder = 5;
    const double plateBackWallCY = 22;
    const double plateFrontWallCY = 12;
    const double plateCX = u1 * 18.5 + plateBorder * 2;
    const double plateCY = u1 * 6.5 + plateBorder * 2;

    // SVG image size in mm  
    const double svgWidth = u1 * 18.5 + plateBorder * 2;
    const double svgHeight = u1 * 6.5 + plateBorder * 2 + plateFrontWallCY + plateBackWallCY;

    bool writeGCode = false;
    bool writeSVG => !writeGCode;


    Program()
    {
      Comment($"Program starts");

      //CutU1(0, 0);
      CutAllKeys();
      //CutStabilBase(0, 0);

      Comment("Program stops");
      JogZ(safeZ);
      JogXY(0, 0);
    }

    void CutAllKeys()
    {
      WriteSVGStart();

      DrawSVGRect(0, 0, plateCX, plateBackWallCY); 
      DrawSVGRect(0, plateBackWallCY, plateCX, plateBackWallCY + plateCY);
      DrawSVGRect(0, plateBackWallCY + plateCY, plateCX, plateBackWallCY + plateCY + plateFrontWallCY);

      // plate is being milled upside-down because thinners are on the inside
      double rowX = plateBorder;
      double rowY = plateBackWallCY + plateBorder; 

      // 1st row: Escape
      CutSwitchUnit(rowX, rowY, 1);
      for (int i = 0; i < 4; i++) CutSwitchU1(rowX + u1 * 2 + i * u1, rowY);
      for (int i = 0; i < 4; i++) CutSwitchU1(rowX + u1 * 6.5 + i * u1, rowY);
      for (int i = 0; i < 4; i++) CutSwitchU1(rowX + u1 * 11 + i * u1, rowY);
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      // 2nd row: digits
      rowY += u1 * 1.5;
      for (int i = 0; i < 13; i++) CutSwitchU1(rowX + i * u1, rowY);
      CutSwitchUnitWithStabilizer(rowX + 13 * u1, rowY, 2, 24); // backspace
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      // 3rd row: Tab
      rowY += u1;
      CutSwitchUnit(rowX, rowY, 1.5); 
      for (int i = 0; i < 12; i++) CutSwitchU1(rowX + u1 * 1.5 + i * u1, rowY);
      CutSwitchUnit(rowX + u1 * 1.5 + 12 * u1, rowY, 1.5);
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      // 4th row: Caps Lock
      rowY += u1;
      CutSwitchUnit(rowX, rowY, 1.75); 
      for (int i = 0; i < 11; i++) CutSwitchU1(rowX + u1 * 1.75 + i * u1, rowY);
      CutSwitchUnitWithStabilizer(rowX + u1 * 1.75 + 11 * u1, rowY, 2.25, 24); // enter

      // 5th row: Shift
      rowY += u1;
      CutSwitchUnitWithStabilizer(rowX, rowY, 2.25, 24); // left shift
      for (int i = 0; i < 10; i++) CutSwitchU1(rowX + u1 * 2.25 + i * u1, rowY);
      CutSwitchUnitWithStabilizer(rowX + u1 * 2.25 + 10 * u1, rowY, 2.75, 24); // right shift
      CutSwitchU1(rowX + u1 * 16.5, rowY); // arrow up

      // 6th row: Space
      rowY += u1;
      for(int i = 0; i < 3; i++) CutSwitchUnit(rowX + i * 1.25 * u1, rowY, 1.25);
      CutSwitchUnitWithStabilizer(rowX + u1 * 3.75, rowY, 6.25, 100); // Space
      for (int i = 0; i < 4; i++) CutSwitchUnit(rowX + u1 * (3.75 + 6.25) + i * 1.25 * u1, rowY, 1.25);
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // arrows

      WriteSVGStop();
    }

    void CutStabilPair(double unitX, double unitY, double widthInUnits, double leverWidth)
    {
      double unitCenterX = unitX + widthInUnits * u1 / 2;
      double leftBaseCenterX = unitCenterX - leverWidth / 2;
      double rightBaseCenterX = leftBaseCenterX + leverWidth;
      double unitCenterY = unitY + u1 / 2;

      CutStabilBase(leftBaseCenterX, unitCenterY);
      CutStabilBase(rightBaseCenterX, unitCenterY);

      // lever pass-through cutout
      double passWidth = millD;
      DrawSVGRect(leftBaseCenterX, unitCenterY, 
                  rightBaseCenterX, unitCenterY + passWidth);

    }

    void CutStabilBase(double centerX, double centerY)
    {
      double holeX1 = centerX - stabilHoleCX / 2;
      double centerToTopCY = (stabilHoleCY - stabilCenterCY);
      double holeY1 = centerY - centerToTopCY; // is cut upside-down
      double holeX2 = holeX1 + stabilHoleCX;
      double holeY2 = holeY1 + stabilHoleCY;

      DrawSVGRect(holeX1, holeY1, holeX2, holeY2);
    }

    void CutSwitchU1(double unitX, double unitY) => CutSwitchUnit(unitX, unitY, 1);

    void CutSwitchUnitWithStabilizer(double unitX, double unitY, double widthInUnits, double stabilWidth)
    {
      CutSwitchUnit(unitX, unitY, widthInUnits);
      CutStabilPair(unitX, unitY, widthInUnits, stabilWidth);
    }

    void CutSwitchUnit(double unitX, double unitY, double widthInUnits)
    {
      // границы отверстия
      double holeX1 = unitX + ((u1 * widthInUnits) - switchHoleCX) / 2;
      double holeX2 = holeX1 + switchHoleCX;
      double holeY1 = unitY + (u1 - switchHoleCY) / 2;
      double holeY2 = holeY1 + switchHoleCY;

      DrawSVGRect(unitX, unitY, unitX + u1 * widthInUnits, unitY + u1); // unit area
      DrawSVGRect(holeX1, holeY1, holeX2, holeY2);                      // switch cutout

      JogZ(safeZ);
      Comment("Unit pocket");
      CutSquare(true, holeX1, holeY1 - switchThinnerCY, holeX2, holeY2 + switchThinnerCY, switchThinnerDepthZ);

      double topZ = switchThinnerDepthZ;
      double bottomZ = finalCutDepthZ;
      CutSquareMultiPass(holeX1, holeY1, holeX2, holeY2, topZ, bottomZ);

      double off = millR * (1 - Math.Cos(Math.PI / 4));
      Comment($"Corner00 off={off}");
      CutSafeHole(holeX1 - off + millR, holeY1 - off + millR, finalCutDepthZ);
      Comment("Corner01");
      CutSafeHole(holeX1 - off + millR, holeY2 + off - millR, finalCutDepthZ);
      Comment("Corner11");
      CutSafeHole(holeX2 + off - millR, holeY2 + off - millR, finalCutDepthZ);
      Comment("Corner10");
      CutSafeHole(holeX2 + off - millR, holeY1 - off + millR, finalCutDepthZ);
    }

    void CutSquareFullDepth(double x1, double y1, double x2, double y2)
      => CutSquareMultiPass(x1, y1, x2, y2, plateTopZ, finalCutDepthZ);

    void CutSquareMultiPass(double x1, double y1, double x2, double y2, double topZ, double bottomZ)
    {
      bool jogToStart = true;
      int numSteps = (int)((topZ - bottomZ) / proposedStepDown);
      double realStepDownZ = (topZ - bottomZ) / numSteps;
      for (int i = 0; i < numSteps; i++) {
        Comment($"Cut {i + 1}/{numSteps} zDown={D2S(realStepDownZ)}");
        double cutZ = topZ - (i + 1) * realStepDownZ;
        CutSquare(jogToStart, x1, y1, x2, y2, cutZ);
        jogToStart = false;
      }
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
