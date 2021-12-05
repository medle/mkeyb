
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MKeybGCoder
{
  class Program
  {
    #region Setup

    static StreamWriter outputWriter;

    static void Main(string[] args)
    {
      bool gcode = (args.Length == 1 && args[0] == "-g");

      var memoryStream = new MemoryStream();
      outputWriter = new StreamWriter(memoryStream);

      new Program(gcode);

      outputWriter.Flush();
      memoryStream.Position = 0;

      // in gcode mode output to console
      if (gcode) {
        var reader = new StreamReader(memoryStream);
        while (true) {
          string s = reader.ReadLine();
          if (s == null) break;
          Console.WriteLine(s);
        }
      }
      else { // in SVG mode show file
        string imageFileName = "image.svg";
        using (FileStream fileStream = new FileStream(imageFileName, FileMode.OpenOrCreate, FileAccess.Write)) {
          memoryStream.CopyTo(fileStream);
        }
        System.Diagnostics.Process.Start(imageFileName);
      }
    }

    // mill bit
    const double millD = 3;
    const double millR = millD / 2;
    const double millOvercut = 0.05; // 3mm bit overcuts 0.05mm (decided with new switches)

    // cutting parameters
    const double safeZ = 5;
    const double slowZ = 1;
    const double plateTopZ = 0;
    const double proposedStepDown = 0.3;
    const double finalCutDepthZ = (plateTopZ - 2.2);
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
    const double stabilHoleCX = 6.7 - (millOvercut * 2);
    const double stabilHoleCY = 12.2 - (millOvercut * 2);
    const double stabilCenterCY = 7;
    const double stabilBulgeBottomEdgeCY = stabilCenterCY;
    const double stabilBulgeCX = 0.5;
    const double stabilBulgeCY = 2;
    const double stabilThinnerCY = 1;
    const double stabilThinnerDepthZ = (plateTopZ - 0.2);

    // keyboard plate sizes
    const double plateBorder = (25.4 / 5);
    const double plateCornerRadius = plateBorder;
    const double plateBackWallCY = 20;
    const double plateFrontWallCY = 10;
    const double plateCX = u1 * 18.5 + plateBorder * 2;
    const double plateCY = u1 * 6.5 + plateBorder * 2;
    const double stockOffset = 5;

    // SVG image size in mm  
    const double svgWidth = plateCX + stockOffset * 2;
    const double svgHeight = plateCY + plateFrontWallCY + plateBackWallCY + stockOffset * 2;

    // X coordinate for halving the plate in two parts (computed value = 186.213)
    const double verticalSplitX = (svgWidth / 2);

    // produce GCode or SVG picture
    bool produceGCode = false;
    bool produceSVG => !produceGCode;

    Program(bool gcode)
    {
      produceGCode = gcode;
      Comment($"Program was generated {DateTime.Now.ToString("dd.MM.yy HH:mm")}");
      Comment($"Tool diameter={millD}mm");
      Comment($"View GCode: ncviewer.com");
      WriteSVGStart();
      DrawSVGCircle(0, 0, 2);
      DrawSVGCircle(svgWidth, 0, 2);

      //CutU1(0, 0);
      CutTopPlateOutline();
      CutPlateHoles();
      //CutAllKeys();
      //CutStabilBase(0, 0, true);
      //CutSwitchUnitWithStabilizer(0, 0, 2, 24);
      //CutStabilPair(0, 0, 2, 24);

      //CutKeyCapHolderBase(0, 0, 3, 17.9, 0.9); // row3
      //CutKeyCapHolderClamp(0, 0); 

      Comment("Program stops");
      JogZ(safeZ);
      JogXY(0, 0);

      WriteSVGStop();
    }

    #endregion
    #region KeyCapHolder

    double keyCapClampHolesCenterToCenterCX = 25;
    double keyCapTopCY = 14.5;
    double keyCapTopCX = 12.5;

    void CutKeyCapHolderClamp(double xLeft, double yBottom)
    {
      Comment($"KeyCapHolderClamp");

      double holeCX = 13; // left and right sides are holding
      double holeCY = 17; // top and bottom sides are non-holding due to varying wall angles

      double keyCapGapCY = (holeCY / 2) - (keyCapTopCY / 2);
      double x1 = xLeft + (keyCapClampHolesCenterToCenterCX / 2) - (holeCX / 2);
      double y1 = yBottom - keyCapGapCY; // place yBottom at KeyCapBottom zero
      double x2 = x1 + holeCX;
      double y2 = y1 + holeCY;

      CutSquareMultiPass(x1, y1, x2, y2, 0, finalCutDepthZ);

      DrawSVGRect(x1, y1 + keyCapGapCY, x2, y2 - keyCapGapCY); // key cap top area
      DrawSVGCircle(xLeft, yBottom, 0.5); // mark zero point

      double drillX1 = xLeft;
      double drillX2 = drillX1 + keyCapClampHolesCenterToCenterCX;
      double drillY = y1 + holeCY / 2;

      // clamp skrew holes
      CutSafeHole(drillX1, drillY, finalCutDepthZ);
      CutSafeHole(drillX2, drillY, finalCutDepthZ);

      // reference geometry markings
      CutSafeHole(x1, yBottom + millR, 0.5);
      CutSafeHole(x2, yBottom + millR, 0.5);
    }

    void CutKeyCapHolderBase(
      double zeroX, double zeroY, double bottomToZeroCY, double holeCY, double depthZ)
    {
      Comment($"KeyCapHolderBase offCY={D2S(bottomToZeroCY)} depthZ={D2S(depthZ)}");

      // width and height of keycap bottom square
      double holeCX = 17.9 - (millOvercut * 2);

      double x1 = zeroX + (keyCapClampHolesCenterToCenterCX / 2) - (holeCX / 2);
      double y1 = zeroY - bottomToZeroCY;
      double x2 = x1 + holeCX;
      double y2 = y1 + holeCY;

      CutSquareZZ(true, x1, y1, x2, y2, -depthZ, 0);
      CutSquareZZ(false, x1, y1, x2, y2, -1 - depthZ, -1);
      CutSquareZZ(false, x1, y1, x2, y2, -2 - depthZ, -2);
      OverCutSquareCorners(x1, y1, x2, y2, -2 - depthZ);

      double drillX1 = zeroX;
      double drillX2 = drillX1 + keyCapClampHolesCenterToCenterCX;
      double drillY = zeroY + (keyCapTopCY / 2);

      CutSafeHole(drillX1, drillY, -3);
      CutSafeHole(drillX2, drillY, -3);

      double keyCapX = zeroX + (keyCapClampHolesCenterToCenterCX / 2) - (keyCapTopCX / 2);
      DrawSVGRect(keyCapX, zeroY, keyCapX + keyCapTopCX, zeroY + keyCapTopCY); // key cap top area
      DrawSVGCircle(zeroX, zeroY, 0.5); // mark zero point
    }

    #endregion
    #region Plate

    double GetBoltHoleX(int index)
    {
      double x1 = stockOffset + plateBorder + plateFrontWallCY / 2;
      double x3 = stockOffset + plateCX / 2;
      double dx = (x3 - x1) / 2;
      return x1 + (dx * index);
    }

    void CutPlateHoles()
    {
      double x1 = GetBoltHoleX(1);
      double x3 = GetBoltHoleX(3);
      Comment($"Plate holes x1={D2S(x1)} x3={D2S(x3)} [x1,x3]={D2S(x3 - x1)}");

      double cyFromBottom = plateFrontWallCY / 2;

      double frontY = stockOffset + plateBackWallCY + plateCY + cyFromBottom;
      for(int i=0; i<5; i++) CutSafeHole(GetBoltHoleX(i), frontY, finalCutDepthZ);

      double backY = stockOffset + cyFromBottom;
      for (int i = 0; i < 5; i++) CutSafeHole(GetBoltHoleX(i), backY, finalCutDepthZ);
    }

    void CutTopPlateOutline()
    {
      Comment($"Plate outline splitX={D2S(verticalSplitX)}");

      double x = stockOffset;
      double y = stockOffset;
      double cornerR = plateCornerRadius;

      // back wall
      DrawSVGRect(x + cornerR, y, x + plateCX - cornerR, y + plateBackWallCY);

      // top plate
      y += plateBackWallCY;
      double topY = y + plateCY;

      JogSafeZ();
      JogXY(verticalSplitX, y - plateBackWallCY - millR);
      JogZ(0);

      // plate left cutout bottom-to-top clockwise
      var leftPath = new GPath(verticalSplitX, y - plateBackWallCY - millR);
      leftPath.AddLineTo(x + cornerR + cornerR - millR, y - plateBackWallCY - millR);
      leftPath.AddArc1To(x + cornerR - millR, y - plateBackWallCY + cornerR - millR, cornerR);
      leftPath.AddLineTo(x + cornerR - millR, y - millR);
      leftPath.AddArc1To(x - millR, y + cornerR, cornerR + millR);

      //leftPath.AddLineTo(x - millR, topY - cornerR);
      double bendCY = 5.6;
      double bendR = 7.5;
      double bendCX = 2.5;
      double stepCY = (plateCY - cornerR * 2 - bendCY * 4) / 3;
      leftPath.AddLineTo(leftPath.X, leftPath.Y + stepCY); // bottom step
      leftPath.AddArc1To(leftPath.X + bendCX / 2, leftPath.Y + bendCY, bendR);
      leftPath.AddArc2To(leftPath.X + bendCX / 2, leftPath.Y + bendCY, bendR);
      leftPath.AddLineTo(leftPath.X, leftPath.Y + stepCY); // middle step
      leftPath.AddArc2To(leftPath.X - bendCX / 2, leftPath.Y + bendCY, bendR);
      leftPath.AddArc1To(leftPath.X - bendCX / 2, leftPath.Y + bendCY, bendR);
      leftPath.AddLineTo(leftPath.X, topY - cornerR);

      leftPath.AddArc1To(x + cornerR - millR, topY + millR, cornerR + millR);
      leftPath.AddLineTo(x + cornerR - millR, topY + plateFrontWallCY - cornerR + millR);
      leftPath.AddArc1To(x + cornerR + cornerR - millR, topY + plateFrontWallCY + millR, cornerR);
      leftPath.AddLineTo(verticalSplitX, topY + plateFrontWallCY + millR);
      CutPathZ(leftPath, 0);

      JogSafeZ();
      JogXY(verticalSplitX, topY + plateFrontWallCY + millR);
      JogZ(0);

      // plate right cutout top-to-bottom clockwise
      double rightX = x + plateCX;
      var rightPath = new GPath(verticalSplitX, topY + plateFrontWallCY + millR);
      rightPath.AddLineTo(rightX - cornerR - cornerR + millR, topY + plateFrontWallCY + millR);
      rightPath.AddArc1To(rightX - cornerR + millR, topY + plateFrontWallCY - cornerR + millR, cornerR);
      rightPath.AddLineTo(rightX - cornerR + millR, topY + millR);
      rightPath.AddArc1To(rightX + millR, topY - cornerR + millR, cornerR);
      rightPath.AddLineTo(rightX + millR, y + cornerR - millR);
      rightPath.AddArc1To(rightX + millR - cornerR, y - millR, cornerR);
      rightPath.AddLineTo(rightX + millR - cornerR, y - plateBackWallCY + cornerR - millR);
      rightPath.AddArc1To(rightX + millR - cornerR - cornerR, y - plateBackWallCY - millR, cornerR);
      rightPath.AddLineTo(verticalSplitX, y - plateBackWallCY - millR);
      CutPathZ(rightPath, 0);

      // draw corners
      DrawSVGCircle(x + cornerR, y + cornerR, cornerR);
      DrawSVGCircle(x + cornerR, topY - cornerR, cornerR);
      DrawSVGCircle(rightX - cornerR, topY - cornerR, cornerR);
      DrawSVGCircle(rightX - cornerR, y + cornerR, cornerR);

      // front wall
      y += plateCY;
      DrawSVGRect(x + cornerR, y, x + plateCX - cornerR, y + plateFrontWallCY);
    }

    void CutAllKeys()
    {
      Comment($"plateCX={D2S(plateCX)} plateCY={D2S(plateCY)}");

      // plate is being milled upside-down because thinners are on the inside
      double rowX = stockOffset + plateBorder;
      double rowY = stockOffset + plateBackWallCY + plateBorder;

      Comment("Row 1: Escape");
      CutSwitchUnit(rowX, rowY, 1);
      for (int i = 0; i < 4; i++) CutSwitchU1(rowX + u1 * 2 + i * u1, rowY);
      for (int i = 0; i < 4; i++) CutSwitchU1(rowX + u1 * 6.5 + i * u1, rowY);
      for (int i = 0; i < 4; i++) CutSwitchU1(rowX + u1 * 11 + i * u1, rowY);
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      Comment("Row 2: Digits");
      rowY += u1 * 1.5;
      for (int i = 0; i < 13; i++) CutSwitchU1(rowX + i * u1, rowY);
      CutSwitchUnitWithStabilizer(rowX + 13 * u1, rowY, 2, 24); // backspace
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      Comment("Row 3: Tab");
      rowY += u1;
      CutSwitchUnit(rowX, rowY, 1.5);
      for (int i = 0; i < 12; i++) CutSwitchU1(rowX + u1 * 1.5 + i * u1, rowY);
      CutSwitchUnit(rowX + u1 * 1.5 + 12 * u1, rowY, 1.5);
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // extra

      Comment("Row 4: Caps Lock");
      rowY += u1;
      CutSwitchUnit(rowX, rowY, 1.75);
      for (int i = 0; i < 11; i++) CutSwitchU1(rowX + u1 * 1.75 + i * u1, rowY);
      CutSwitchUnitWithStabilizer(rowX + u1 * 1.75 + 11 * u1, rowY, 2.25, 24); // enter

      Comment("Row 5: Shift");
      rowY += u1;
      CutSwitchUnitWithStabilizer(rowX, rowY, 2.25, 24); // left shift
      for (int i = 0; i < 10; i++) CutSwitchU1(rowX + u1 * 2.25 + i * u1, rowY);
      CutSwitchUnitWithStabilizer(rowX + u1 * 2.25 + 10 * u1, rowY, 2.75, 24); // right shift
      CutSwitchU1(rowX + u1 * 16.5, rowY); // arrow up

      Comment("Row 6: Space");
      rowY += u1;
      for (int i = 0; i < 3; i++) CutSwitchUnit(rowX + i * 1.25 * u1, rowY, 1.25);
      CutSwitchUnitWithStabilizer(rowX + u1 * 3.75, rowY, 6.25, 100); // Space
      for (int i = 0; i < 4; i++) CutSwitchUnit(rowX + u1 * (3.75 + 6.25) + i * 1.25 * u1, rowY, 1.25);
      for (int i = 0; i < 3; i++) CutSwitchU1(rowX + u1 * 15.5 + i * u1, rowY); // arrows
    }

    void CutSwitchU1(double unitX, double unitY) => CutSwitchUnit(unitX, unitY, 1);

    void CutSwitchUnitWithStabilizer(double unitX, double unitY, double widthInUnits, double stabilWidth)
    {
      CutSwitchUnit(unitX, unitY, widthInUnits);
      CutStabilPair(unitX, unitY, widthInUnits, stabilWidth);
    }

    void CutStabilPair(double unitX, double unitY, double widthInUnits, double leverWidth)
    {
      Comment($"StabilPair {widthInUnits}");
      double unitCenterX = unitX + widthInUnits * u1 / 2;
      double leftBaseCenterX = unitCenterX - leverWidth / 2;
      double rightBaseCenterX = leftBaseCenterX + leverWidth;
      double unitCenterY = unitY + u1 / 2;

      CutStabilBase(leftBaseCenterX, unitCenterY, true);
      CutStabilBase(rightBaseCenterX, unitCenterY, false);

      Comment("Stabil lever pass");
      double passCY = stabilBulgeCY * 2;
      double passY = unitCenterY - stabilBulgeCY;
      double leftPassX1 = leftBaseCenterX + stabilHoleCX / 2 - millR;
      double leftPassX2 = unitCenterX - switchHoleCX / 2 + millR;
      CutSquareFullDepth(leftPassX1, passY, leftPassX2, passY + passCY);
      double rightPassX1 = unitCenterX + switchHoleCX / 2 - millR;
      double rightPassX2 = rightBaseCenterX - stabilHoleCX / 2 + millR;
      CutSquareFullDepth(rightPassX1, passY, rightPassX2, passY + passCY);
    }

    void CutStabilBase(double centerX, double centerY, bool isLeftBase)
    {
      double holeX1 = centerX - stabilHoleCX / 2;
      double stabilCenterToTopCY = (stabilHoleCY - stabilCenterCY);
      double holeY1 = centerY - stabilCenterToTopCY; // is cut upside-down
      double holeX2 = holeX1 + stabilHoleCX;
      double holeY2 = holeY1 + stabilHoleCY;

      JogZ(safeZ);
      Comment("Stabilizer base");
      CutSquare(true, holeX1, holeY1 - switchThinnerCY, holeX2, holeY2 + stabilThinnerCY, stabilThinnerDepthZ);
      double topZ = stabilThinnerDepthZ;
      double bottomZ = finalCutDepthZ;
      CutSquareMultiPass(holeX1, holeY1, holeX2, holeY2, topZ, bottomZ);

      JogZ(safeZ);
      Comment("Stabilizer bulges");
      double bulgeCenterY = centerY - (stabilBulgeCY / 2); // is cut upside-down
      if (isLeftBase) CutSafeHole(holeX1, bulgeCenterY, finalCutDepthZ);
      else CutSafeHole(holeX2, bulgeCenterY, finalCutDepthZ);
    }

    void CutSwitchUnit(double unitX, double unitY, double widthInUnits)
    {
      // switch cutout area
      double holeX1 = unitX + ((u1 * widthInUnits) - switchHoleCX) / 2;
      double holeX2 = holeX1 + switchHoleCX;
      double holeY1 = unitY + (u1 - switchHoleCY) / 2;
      double holeY2 = holeY1 + switchHoleCY;

      DrawSVGRect(unitX, unitY, unitX + u1 * widthInUnits, unitY + u1); // unit area

      JogZ(safeZ);
      Comment($"Switch cutout {D2S(widthInUnits)}");
      CutSquare(true, holeX1, holeY1 - switchThinnerCY, holeX2, holeY2 + switchThinnerCY, switchThinnerDepthZ);
      double topZ = switchThinnerDepthZ;
      double bottomZ = finalCutDepthZ;
      CutSquareMultiPassWithCorners(holeX1, holeY1, holeX2, holeY2, topZ, bottomZ);
    }

    #endregion
    #region Util

    double CornerOverCutDiagonalOffset => millR * (1 - Math.Cos(Math.PI / 4));

    void CutSquareMultiPassWithCorners(
      double holeX1, double holeY1, double holeX2, double holeY2, double topZ, double bottomZ)
    {
      CutSquareMultiPass(holeX1, holeY1, holeX2, holeY2, topZ, bottomZ);
      OverCutSquareCorners(holeX1, holeY1, holeX2, holeY2, bottomZ);
    }

    void OverCutSquareCorners(
      double holeX1, double holeY1, double holeX2, double holeY2, double bottomZ)
    {
      double off = CornerOverCutDiagonalOffset;
      Comment($"Corner00 off={D2S(off)}");
      CutSafeHole(holeX1 - off + millR, holeY1 - off + millR, bottomZ);
      Comment("Corner01");
      CutSafeHole(holeX1 - off + millR, holeY2 + off - millR, bottomZ);
      Comment("Corner11");
      CutSafeHole(holeX2 + off - millR, holeY2 + off - millR, bottomZ);
      Comment("Corner10");
      CutSafeHole(holeX2 + off - millR, holeY1 - off + millR, bottomZ);
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
      DrawSVGCircle(x, y, millR);

      JogZ(safeZ);
      JogXY(x, y);
      JogZ(slowZ);
      CutToF(x, y, finalZ, drillSpeedF);
      JogZ(safeZ);
    }

    void CutSquare(bool jogToStart, double x1, double y1, double x2, double y2, double z)
      => CutSquareZZ(jogToStart, x1, y1, x2, y2, z, z);

    void CutSquareZZ(bool jogToStart, double x1, double y1, double x2, double y2, double z1, double z2)
    {
      DrawSVGRect(x1, y1, x2, y2);

      double millX1 = x1 + millR;
      double millY1 = y1 + millR;
      double millX2 = x2 - millR;
      double millY2 = y2 - millR;

      if (jogToStart) {
        JogZ(safeZ);
        JogXY(millX1, millY1);
        JogZ(slowZ);
      }

      CutToF(millX1, millY1, z1, drillSpeedF);
      CutTo(millX1, millY2, z2); // up (z1 -> z2)
      CutTo(millX2, millY2, z2); // right
      CutTo(millX2, millY1, z1); // down (z2 -> z1)
      CutTo(millX1, millY1, z1); // left
    }

    void CutPathZ(GPath path, double z)
    {
      foreach (var segment in path.Segments) {
        if (segment is GPath.LineSegment line) CutLineSegment(line, z);
        if (segment is GPath.ArcSegment arc) CutArcSegment(arc, z);
      }
    }

    void CutLineSegment(GPath.LineSegment line, double z)
    {
      DrawSVGCircle(line.FromX, line.FromY, millR);
      DrawSVGLine(line.FromX, line.FromY, line.ToX, line.ToY);
      DrawSVGCircle(line.ToX, line.ToY, millR);

      CutTo(line.ToX, line.ToY, z);
    }

    void CutArcSegment(GPath.ArcSegment arc, double z)
    {
      DrawSVGCircle(arc.FromX, arc.FromY, millR);
      DrawSVGArc(arc);
      DrawSVGCircle(arc.ToX, arc.ToY, millR);

      int gcode = arc.Clockwise ? 2 : 3;
      WriteGCodeLine($"G0{gcode} X{D2S(arc.ToX)} Y{D2S(arc.ToY)} R{D2S(arc.Radius)} {cuttingSpeedF}");
    }

    void JogSafeZ() => JogZ(safeZ);

    void JogZ(double z) => WriteGCodeLine($"G0 Z{D2S(z)}");

    void JogXY(double x, double y) => WriteGCodeLine($"G0 X{D2S(x)} Y{D2S(y)}");

    void CutTo(double x, double y, double z) => CutToF(x, y, z, cuttingSpeedF);

    void CutToF(double x, double y, double z, string speedF)
      => WriteGCodeLine($"G1 X{D2S(x)} Y{D2S(y)} Z{D2S(z)} {speedF}");

    void Comment(string s) 
    {
      if (produceGCode) WriteGCodeLine($"({s})");
      WriteSVG($"<!-- {s} -->");
    }

    void WriteGCodeLine(string s)
    {
      if (produceGCode) WriteLine(s);
    }

    readonly NumberFormatInfo dotNFI =
      new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };

    string D2S(double d) => string.Format(dotNFI, "{0:N}", d);

    void DrawSVGLine(double x1, double y1, double x2, double y2)
    {
      // SVG Y-coordinate goes top-down, GCode Y-coordinate goes down-top
      WriteSVG(
        $"<line x1='{D2S(x1)}' y1='{D2S(svgHeight - y1)}'" +
        $" x2='{D2S(x2)}' y2='{D2S(svgHeight - y2)}'" +
        $" stroke='green' stroke-width='0.1' />");
    }

    void DrawSVGCircle(double centerX, double centerY, double radius)
    {
      // SVG Y-coordinate goes top-down, GCode Y-coordinate goes down-top
      WriteSVG(
        $"<circle cx='{D2S(centerX)}' cy='{D2S(svgHeight - centerY)}' r='{D2S(radius)}'" +
        $" stroke='darkred' stroke-width='0.1' fill='none' />"
        );
    }

    void DrawSVGRect(double x1, double y1, double x2, double y2)
    {
      // SVG Y-coordinate goes top-down, GCode Y-coordinate goes down-top
      WriteSVG(
        $"<rect x='{D2S(x1)}' y='{D2S(svgHeight - y2)}' width='{D2S(x2 - x1)}'" +
        $" height='{D2S(y2 - y1)}' stroke='black' stroke-width='0.1' fill='none' />");
    }

    void DrawSVGArc(GPath.ArcSegment arc)
    {
      // SVG Y-coordinate goes top-down, GCode Y-coordinate goes down-top
      int flag = arc.Clockwise ? 1 : 0;
      WriteSVG(
        $"<path d='"  +
        $"M {D2S(arc.FromX)} {D2S(svgHeight - arc.FromY)}" +
        $" A {D2S(arc.Radius)} {D2S(arc.Radius)} 0 0 {flag} {D2S(arc.ToX)} {D2S(svgHeight - arc.ToY)}" +
        $"' stroke='green' stroke-width='0.1' fill='none' />");
    }

    void WriteSVGStart()
    {
      WriteSVG($"<svg xmlns='http://www.w3.org/2000/svg'");
      WriteSVG($" width='{D2S(svgWidth)}mm' height='{D2S(svgHeight)}mm'");
      WriteSVG($" viewBox='0 0 {D2S(svgWidth)} {D2S(svgHeight)}'>");
    }

    void WriteSVGStop() => WriteSVG("</svg>");

    void WriteSVG(string s)
    {
      if (produceSVG) WriteLine(s);
    }

    void WriteLine(string s)
    {
      //Console.WriteLine(s);
      outputWriter.WriteLine(s);
    }

    #endregion
  }
}
