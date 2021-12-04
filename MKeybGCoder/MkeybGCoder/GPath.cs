
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKeybGCoder
{
  public class GPath
  {
    public double StartX;
    public double StartY;

    public List<Segment> Segments = new List<Segment>();

    public GPath(double startX, double startY)
    {
      StartX = startX;
      StartY = startY;
    }

    double LastX => (Segments.Count == 0) ? StartX : Segments.Last().ToX;
    double LastY => (Segments.Count == 0) ? StartY : Segments.Last().ToY;

    public void AddLineTo(double toX, double toY)
      => this.Segments.Add(new LineSegment(LastX, LastY, toX, toY));

    public void AddArc1To(double toX, double toY, double radius)
      => this.Segments.Add(new ArcSegment(LastX, LastY, toX, toY, radius, true));

    public void AddArcTo(double toX, double toY, double radius, bool clockwise)
      => this.Segments.Add(new ArcSegment(LastX, LastY, toX, toY, radius, clockwise));

    public class Segment
    {
      public double FromX;
      public double FromY;
      public double ToX;
      public double ToY;

      public Segment(double fromX, double fromY, double toX, double toY)
      {
        this.FromX = fromX;
        this.FromY = fromY;
        this.ToX = toX;
        this.ToY = toY;
      }
    }

    public class LineSegment : Segment 
    {
      public LineSegment(double fromX, double fromY, double toX, double toY) : 
        base(fromX, fromY, toX, toY)
      {
      }
    }

    public class ArcSegment : Segment 
    {
      public double Radius;
      public bool Clockwise;

      public ArcSegment(double fromX, double fromY, double toX, double toY, double radius, bool clockwise) :
        base(fromX, fromY, toX, toY)
      {
        this.Radius = radius;
        this.Clockwise = clockwise;
      }
    }
  }
}
