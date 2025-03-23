using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
namespace Lab4
{


    public partial class Form1 : Form
    {
        private readonly ShapeContainer _shapeContainer = new ShapeContainer();
        private Shape _selectedShape; // Текущая выбранная фигура (для перетаскивания)
        private Point _mouseDownLocation; // Точка, где была нажата мышь
        private bool _isDragging; // Флаг, указывающий на процесс перетаскивания
        private ShapeType _selectedShapeType = ShapeType.Circle; // Тип фигуры, выбранный в панели инструментов
        private readonly ColorDialog _colorDialog = new ColorDialog();
        private const int ShapeSize = 50;  // Default size for new shapes
        private bool _ctrlKeyPressed = false; // Флаг для состояния клавиши Ctrl
        private List<Shape> _selectedShapes = new List<Shape>(); // Список выделенных фигур

        // Перечисление для типов фигур
        public enum ShapeType
        {
            Circle,
            Rectangle,
            Ellipse,
            Pentagon,
            Line
        }
        public Form1()
        {
            InitializeComponent();
            // Настройка панели инструментов (можно добавить кнопки для выбора типов фигур)
            AddShapeTypeButtons();
            DoubleBuffered = true; // Для устранения мерцания
            this.KeyPreview = true; // Важно: позволяет форме перехватывать нажатия клавиш
            this.KeyDown += Form1_KeyDown; // Подписываемся на события KeyDown и KeyUp
            this.KeyUp += Form1_KeyUp;
        }
        private void AddShapeTypeButtons()
        {
            ToolStripButton circleButton = new ToolStripButton("Круг");
            circleButton.Tag = ShapeType.Circle;
            circleButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(circleButton);

            ToolStripButton rectangleButton = new ToolStripButton("Прямоугольник");
            rectangleButton.Tag = ShapeType.Rectangle;
            rectangleButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(rectangleButton);

            ToolStripButton ellipseButton = new ToolStripButton("Эллипс");
            ellipseButton.Tag = ShapeType.Ellipse;
            ellipseButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(ellipseButton);

            ToolStripButton pentagonButton = new ToolStripButton("Пятиугольник");
            pentagonButton.Tag = ShapeType.Pentagon;
            pentagonButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(pentagonButton);

            ToolStripButton lineButton = new ToolStripButton("Линия");
            lineButton.Tag = ShapeType.Line;
            lineButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(lineButton);

            toolStrip1.Items.Add(new ToolStripSeparator());

            ToolStripButton colorButton = new ToolStripButton("Цвет");
            colorButton.Click += ChangeColorButton_Click;
            toolStrip1.Items.Add(colorButton);

            ToolStripButton clearSelectionButton = new ToolStripButton("Снять выделение");
            clearSelectionButton.Click += ClearSelectionButton_Click;
            toolStrip1.Items.Add(clearSelectionButton);

            ToolStripButton resetButton = new ToolStripButton("Сброс");
            resetButton.Click += ResetButton_Click;
            toolStrip1.Items.Add(resetButton);
        }
        private void ClearSelectionButton_Click(object sender, EventArgs e)
        {
            ClearSelectedShapes();
            Invalidate();
        }

        private void ShapeTypeButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripButton button && button.Tag is ShapeType type)
            {
                _selectedShapeType = type;
            }
        }
        private void ResetButton_Click(object sender, EventArgs e)
        {
            // Очищаем контейнер фигур и список выделенных
            _shapeContainer.Clear(); // Используем публичный метод Clear()
            ClearSelectedShapes();
            Invalidate(); // Перерисовываем форму
        }

        private void ChangeColorButton_Click(object sender, EventArgs e)
        {
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color selectedColor = _colorDialog.Color;

                foreach (Shape shape in _shapeContainer.GetSelectedShapes())
                {
                    shape.Color = selectedColor;
                }
                Invalidate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            _shapeContainer.DrawAllShapes(e.Graphics);
            // Отображаем рамку для выделенных фигур
            foreach (Shape shape in _selectedShapes)
            {
                if (shape != null && shape.IsSelected)
                {
                    using (Pen selectionPen = new Pen(Color.Black, 2))
                    {
                        e.Graphics.DrawPath(selectionPen, shape.GetPath());
                    }

                }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Shape clickedShape = _shapeContainer.FindShape(e.Location);

                if (!_ctrlKeyPressed)
                {
                    ClearSelectedShapes();
                }

                if (_ctrlKeyPressed)
                {

                    if (clickedShape != null)
                    {
                        // Toggle the selection state of the clicked shape
                        if (_selectedShapes.Contains(clickedShape))
                        {
                            _selectedShapes.Remove(clickedShape);
                            clickedShape.IsSelected = false;
                        }
                        else
                        {
                            _selectedShapes.Add(clickedShape);
                            clickedShape.IsSelected = true;
                        }
                        _selectedShape = null;
                        _isDragging = false;

                    }
                    else
                    {
                        ClearSelectedShapes();
                        CreateShape(e.Location);
                        Shape newShape = _shapeContainer.FindShape(e.Location);
                        newShape.IsSelected = true;
                        _selectedShapes.Add(newShape);
                        _selectedShape = newShape;
                        _isDragging = true;
                    }

                }
                else
                {
                    if (clickedShape == null)
                    {
                        ClearSelectedShapes();
                        CreateShape(e.Location);
                        Shape newShape = _shapeContainer.FindShape(e.Location);
                        newShape.IsSelected = true;
                        _selectedShapes.Add(newShape);
                        _selectedShape = newShape;
                        _isDragging = true;
                    }
                    else
                    {
                        ClearSelectedShapes();
                        clickedShape.IsSelected = true;
                        _selectedShapes.Add(clickedShape);
                        _selectedShape = clickedShape;
                        _isDragging = true;
                    }
                }

                Invalidate();
                _mouseDownLocation = e.Location;

            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedShape != null)
            {
                int dx = e.X - _mouseDownLocation.X;
                int dy = e.Y - _mouseDownLocation.Y;

                foreach (Shape shape in _selectedShapes)
                {
                    shape.Move(dx, dy, ClientRectangle);
                }

                _mouseDownLocation = e.Location;
                Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _selectedShape = null;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                _ctrlKeyPressed = true;
            }

            List<Shape> shapesToUpdate = _selectedShapes;

            if (shapesToUpdate.Count > 0)
            {
                int moveStep = 5;
                int resizeStep = 5;

                switch (e.KeyCode)
                {
                    case Keys.Up:
                        foreach (var shape in shapesToUpdate)
                            shape.Move(0, -moveStep, ClientRectangle);
                        break;
                    case Keys.Down:
                        foreach (var shape in shapesToUpdate)
                            shape.Move(0, moveStep, ClientRectangle);
                        break;
                    case Keys.Left:
                        foreach (var shape in shapesToUpdate)
                            shape.Move(-moveStep, 0, ClientRectangle);
                        break;
                    case Keys.Right:
                        foreach (var shape in shapesToUpdate)
                            shape.Move(moveStep, 0, ClientRectangle);
                        break;
                    case Keys.Add:
                        foreach (var shape in shapesToUpdate)
                            shape.Resize(resizeStep, resizeStep, ClientRectangle);
                        break;
                    case Keys.Subtract:
                        foreach (var shape in shapesToUpdate)
                            shape.Resize(-resizeStep, -resizeStep, ClientRectangle);
                        break;
                    case Keys.Delete:
                        DeleteSelectedShapes();
                        break;
                }
                Invalidate();
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                _ctrlKeyPressed = false;
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void CreateShape(Point location)
        {
            switch (_selectedShapeType)
            {
                case ShapeType.Circle:
                    _shapeContainer.AddShape(new Circle(Color.Red, location, ShapeSize / 2));
                    break;
                case ShapeType.Rectangle:
                    _shapeContainer.AddShape(new RectangleShape(Color.Green, location, ShapeSize, ShapeSize));
                    break;
                case ShapeType.Ellipse:
                    _shapeContainer.AddShape(new Ellipse(Color.Yellow, location, ShapeSize, ShapeSize / 2));
                    break;
                case ShapeType.Pentagon:
                    _shapeContainer.AddShape(new Pentagon(Color.Purple, location, ShapeSize));
                    break;
                case ShapeType.Line:
                    // Для линии нужно указать начальную и конечную точку
                    Point startPoint = location; // Начальная точка - точка клика
                    Point endPoint = new Point(location.X + ShapeSize, location.Y + ShapeSize); // Конечная точка (пример)
                    _shapeContainer.AddShape(new Line(Color.Orange, startPoint, endPoint));
                    break;
            }
            Invalidate();
        }
        private void DeleteSelectedShapes()
        {
            for (int i = _selectedShapes.Count - 1; i >= 0; i--)
            {
                Shape shape = _selectedShapes[i];
                _shapeContainer.RemoveShape(shape);
                shape.IsSelected = false;
                _selectedShapes.RemoveAt(i);

            }
            ClearSelectedShapes();
            Invalidate();
        }
        private void ClearSelectedShapes()
        {
            foreach (Shape shape in _selectedShapes)
            {
                if (shape != null)
                    shape.IsSelected = false;
            }
            _selectedShapes.Clear();
            _selectedShape = null;
            _isDragging = false; //Добавлено сброс перетаскивания при снятии выделения
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }

    // Базовый класс для всех фигур
    public abstract class Shape
    {
        public Color Color { get; set; }
        public Point Location { get; set; }
        public bool IsSelected { get; set; } // Флаг для выделения фигуры

        protected Shape(Color color, Point location)
        {
            Color = color;
            Location = location;
            IsSelected = false;
        }

        // Метод для отрисовки фигуры (должен быть переопределен в производных классах)
        public abstract void Draw(Graphics g);

        // Метод для проверки попадания точки в фигуру (должен быть переопределен)
        public abstract bool Contains(Point point);

        public abstract GraphicsPath GetPath();
        // Метод для перемещения фигуры
        public virtual void Move(int dx, int dy, Rectangle clientRectangle)
        {
            Rectangle bounds = GetBounds();
            Point newLocation = new Point(Location.X + dx, Location.Y + dy);
            Rectangle newBounds = new Rectangle(newLocation.X, newLocation.Y, bounds.Width, bounds.Height);

            // Check if the new bounds are fully within the client rectangle
            if (clientRectangle.Contains(newBounds))
            {
                Location = newLocation; // Apply the move if within bounds
            }
            else
            {
                // Calculate the maximum allowed movement to stay within bounds
                int allowedDx = 0, allowedDy = 0;

                if (newBounds.Left < clientRectangle.Left)
                    allowedDx = clientRectangle.Left - bounds.Left;
                else if (newBounds.Right > clientRectangle.Right)
                    allowedDx = clientRectangle.Right - bounds.Right;
                else
                    allowedDx = dx; // Full move allowed in X direction

                if (newBounds.Top < clientRectangle.Top)
                    allowedDy = clientRectangle.Top - bounds.Top;
                else if (newBounds.Bottom > clientRectangle.Bottom)
                    allowedDy = clientRectangle.Bottom - bounds.Bottom;
                else
                    allowedDy = dy; // Full move allowed in Y direction

                // Only apply the partial move if there is a valid change
                if (allowedDx != 0 || allowedDy != 0)
                {
                    // Create the new bounds based on partial move
                    Point partialLocation = new Point(Location.X + allowedDx, Location.Y + allowedDy);
                    Rectangle partialBounds = new Rectangle(partialLocation.X, partialLocation.Y, bounds.Width, bounds.Height);

                    // Make sure the partial movement still results in the shape within the client rectangle
                    if (clientRectangle.Contains(partialBounds))
                    {
                        Location = partialLocation;
                    }
                }
            }
        }

        // Метод для изменения размера (должен быть переопределен)
        public abstract void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle);

        // Метод для получения границ фигуры (должен быть переопределен)
        public abstract Rectangle GetBounds();
    }

    // Класс для круга
    public class Circle : Shape
    {
        public int Radius { get; set; }

        public Circle(Color color, Point location, int radius) : base(color, new Point(location.X - radius, location.Y - radius))
        {
            Radius = radius;
        }
        public override GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(Location.X, Location.Y, 2 * Radius, 2 * Radius);
            return path;
        }
        public override void Draw(Graphics g)
        {
            Brush brush = new SolidBrush(Color);
            g.FillEllipse(brush, Location.X, Location.Y, 2 * Radius, 2 * Radius);

            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2); // Рамка для выделения
                g.DrawEllipse(selectionPen, Location.X, Location.Y, 2 * Radius, 2 * Radius);
            }
        }

        public override bool Contains(Point point)
        {
            double dx = point.X - Location.X - Radius;
            double dy = point.Y - Location.Y - Radius;
            return dx * dx + dy * dy <= Radius * Radius;
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            int newRadius = Radius + deltaWidth / 2;
            if (newRadius > 0)
            {
                int x = Location.X;
                int y = Location.Y;
                int diameter = 2 * newRadius;
                if (x >= clientRectangle.Left && y >= clientRectangle.Top && x + diameter <= clientRectangle.Right && y + diameter <= clientRectangle.Bottom)
                {
                    Radius = newRadius;
                }
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(Location.X, Location.Y, 2 * Radius, 2 * Radius);
        }
    }

    // Класс для прямоугольника
    public class RectangleShape : Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public RectangleShape(Color color, Point location, int width, int height) : base(color, new Point(location.X - width / 2, location.Y - height / 2))
        {
            Width = width;
            Height = height;
        }
        public override GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new Rectangle(Location.X, Location.Y, Width, Height));
            return path;
        }
        public override void Draw(Graphics g)
        {
            Brush brush = new SolidBrush(Color);
            g.FillRectangle(brush, Location.X, Location.Y, Width, Height);
            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2);
                g.DrawRectangle(selectionPen, Location.X, Location.Y, Width, Height);
            }
        }

        public override bool Contains(Point point)
        {
            return point.X >= Location.X && point.X <= Location.X + Width &&
                   point.Y >= Location.Y && point.Y <= Location.Y + Height;
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            int newWidth = Width + deltaWidth;
            int newHeight = Height + deltaHeight;

            if (newWidth > 0 && newHeight > 0)
            {
                if (Location.X >= clientRectangle.Left && Location.Y >= clientRectangle.Top && Location.X + newWidth <= clientRectangle.Right && Location.Y + newHeight <= clientRectangle.Bottom)
                {
                    Width = newWidth;
                    Height = newHeight;
                }
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(Location.X, Location.Y, Width, Height);
        }
    }

    // Класс для эллипса
    public class Ellipse : Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Ellipse(Color color, Point location, int width, int height) : base(color, new Point(location.X - width / 2, location.Y - height / 2))
        {
            Width = width;
            Height = height;
        }

        public override GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(Location.X, Location.Y, Width, Height);
            return path;
        }
        public override void Draw(Graphics g)
        {
            Brush brush = new SolidBrush(Color);
            g.FillEllipse(brush, Location.X, Location.Y, Width, Height);
            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2);
                g.DrawEllipse(selectionPen, Location.X, Location.Y, Width, Height);
            }
        }

        public override bool Contains(Point point)
        {
            double dx = point.X - Location.X - Width / 2.0;
            double dy = point.Y - Location.Y - Height / 2.0;
            return (dx * dx) / ((Width / 2.0) * (Width / 2.0)) + (dy * dy) / ((Height / 2.0) * (Height / 2.0)) <= 1;
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            int newWidth = Width + deltaWidth;
            int newHeight = Height + deltaHeight;

            if (newWidth > 0 && newHeight > 0)
            {
                if (Location.X >= clientRectangle.Left && Location.Y >= clientRectangle.Top && Location.X + newWidth <= clientRectangle.Right && Location.Y + newHeight <= clientRectangle.Bottom)
                {
                    Width = newWidth;
                    Height = newHeight;
                }
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(Location.X, Location.Y, Width, Height);
        }
    }

    // Класс для пятиугольника
    public class Pentagon : Shape
    {
        public int Size { get; set; }

        public Pentagon(Color color, Point location, int size) : base(color, location)
        {
            Size = size;
        }
        public override GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            Point[] points = new Point[5];
            double angle = 2 * Math.PI / 5; // Угол между вершинами

            for (int i = 0; i < 5; i++)
            {
                double angleOffset = angle * i - Math.PI / 2;
                points[i] = new Point(
                    Location.X + (int)(Size / 2 * Math.Cos(angleOffset)),
                    Location.Y + (int)(Size / 2 * Math.Sin(angleOffset))
                );
            }
            path.AddPolygon(points);
            return path;
        }
        public override void Draw(Graphics g)
        {
            // Рассчитываем координаты вершин пятиугольника
            Point[] points = new Point[5];
            for (int i = 0; i < 5; i++)
            {
                double angle = 2 * Math.PI * i / 5 - Math.PI / 2; // Сдвиг для ориентации вершины вверх
                int x = Location.X + (int)(Size / 2 * Math.Cos(angle));
                int y = Location.Y + (int)(Size / 2 * Math.Sin(angle));
                points[i] = new Point(x, y);
            }

            Brush brush = new SolidBrush(Color);
            g.FillPolygon(brush, points);

            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2);
                g.DrawPolygon(selectionPen, points);
            }
        }

        public override bool Contains(Point point)
        {
            // Алгоритм "Ray Casting" для определения попадания точки внутрь многоугольника
            double angle = 2 * Math.PI / 5; // Угол между вершинами

            Point[] points = new Point[5];
            for (int i = 0; i < 5; i++)
            {
                double angleOffset = angle * i - Math.PI / 2;
                points[i] = new Point(
                    Location.X + (int)(Size / 2 * Math.Cos(angleOffset)),
                    Location.Y + (int)(Size / 2 * Math.Sin(angleOffset))
                );
            }

            // Используем метод Contains из GraphicsPath для определения, находится ли точка внутри пятиугольника
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddPolygon(points);
                return path.IsVisible(point);
            }
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            int newSize = Size + deltaWidth; // Assuming uniform scaling
            if (newSize > 0)
            {
                // Calculate the bounding box for the new size.
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                Point[] points = new Point[5];
                for (int i = 0; i < 5; i++)
                {
                    double angle = 2 * Math.PI * i / 5 - Math.PI / 2;
                    int x = Location.X + (int)(newSize / 2 * Math.Cos(angle));
                    int y = Location.Y + (int)(newSize / 2 * Math.Sin(angle));
                    points[i] = new Point(x, y);

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }

                // Check if the new bounding box fits within the client rectangle
                if (minX >= clientRectangle.Left && minY >= clientRectangle.Top && maxX <= clientRectangle.Right && maxY <= clientRectangle.Bottom)
                {
                    Size = newSize;
                }
            }
        }

        public override Rectangle GetBounds()
        {
            // Рассчитываем координаты вершин пятиугольника
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            Point[] points = new Point[5];
            for (int i = 0; i < 5; i++)
            {
                double angle = 2 * Math.PI * i / 5 - Math.PI / 2;
                int x = Location.X + (int)(Size / 2 * Math.Cos(angle));
                int y = Location.Y + (int)(Size / 2 * Math.Sin(angle));
                points[i] = new Point(x, y);

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }

    // Класс для отрезка
    public class Line : Shape
    {
        public Point EndPoint { get; set; }

        public Line(Color color, Point startPoint, Point endPoint) : base(color, startPoint)
        {
            EndPoint = endPoint;
        }
        public override GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddLine(Location, EndPoint);
            return path;
        }
        public override void Draw(Graphics g)
        {
            Pen pen = new Pen(Color, 2);
            g.DrawLine(pen, Location, EndPoint);

            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2);
                g.DrawLine(selectionPen, Location, EndPoint);
            }
        }

        public override bool Contains(Point point)
        {
            // Упрощенная проверка на попадание в окрестность линии
            double distance = DistancePointToLine(point, Location, EndPoint);
            return distance <= 3; // Допустимая погрешность
        }

        private double DistancePointToLine(Point point, Point lineStart, Point lineEnd)
        {
            double x0 = point.X;
            double y0 = point.Y;
            double x1 = lineStart.X;
            double y1 = lineStart.Y;
            double x2 = lineEnd.X;
            double y2 = lineEnd.Y;

            double numerator = Math.Abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1));
            double denominator = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            return numerator / denominator;
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            // Сохраняем текущую длину линии
            double currentLength = Math.Sqrt(Math.Pow(EndPoint.X - Location.X, 2) + Math.Pow(EndPoint.Y - Location.Y, 2));

            // Вычисляем новый размер
            double newLength = currentLength + deltaWidth;

            if (newLength > 0)
            {
                // Вычисляем угол наклона линии
                double angle = Math.Atan2(EndPoint.Y - Location.Y, EndPoint.X - Location.X);

                // Вычисляем новую конечную точку на основе этого угла и нового размера
                int newX = Location.X + (int)(newLength * Math.Cos(angle));
                int newY = Location.Y + (int)(newLength * Math.Sin(angle));

                // Проверяем, чтобы новые координаты не выходили за границы
                if (newX >= 0 && newX <= clientRectangle.Width && newY >= 0 && newY <= clientRectangle.Height)
                {
                    EndPoint = new Point(newX, newY);
                }
            }
        }


        public override void Move(int dx, int dy, Rectangle clientRectangle)
        {
            // Move both start and end points
            Point newStart = new Point(Location.X + dx, Location.Y + dy);
            Point newEnd = new Point(EndPoint.X + dx, EndPoint.Y + dy);

            // Make sure the new positions are within bounds before moving
            Rectangle bounds = new Rectangle(Math.Min(newStart.X, newEnd.X), Math.Min(newStart.Y, newEnd.Y), Math.Abs(newStart.X - newEnd.X), Math.Abs(newStart.Y - newEnd.Y));
            if (clientRectangle.Contains(bounds))
            {
                Location = newStart;
                EndPoint = newEnd;
            }
        }



        public override Rectangle GetBounds()
        {
            // Вычисление границ, охватывающих отрезок.
            int minX = Math.Min(Location.X, EndPoint.X);
            int minY = Math.Min(Location.Y, EndPoint.Y);
            int maxX = Math.Max(Location.X, EndPoint.X);
            int maxY = Math.Max(Location.Y, EndPoint.Y);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }

    // Контейнер для фигур
    public class ShapeContainer
    {
        private readonly List<Shape> _shapes = new List<Shape>();
        public Color SelectedColor = Color.Red; // Цвет выделения

        public int Count => _shapes.Count;

        public void AddShape(Shape shape)
        {
            _shapes.Add(shape);
        }

        public void RemoveShape(Shape shape)
        {
            _shapes.Remove(shape);
        }

        public Shape GetShapeAt(int index)
        {
            if (index >= 0 && index < _shapes.Count)
            {
                return _shapes[index];
            }
            return null;
        }

        public void ClearSelection()
        {
            foreach (var shape in _shapes)
            {
                shape.IsSelected = false;
            }
        }

        public void SelectShape(Shape shape)
        {
            ClearSelection();
            if (shape != null)
            {
                shape.IsSelected = true;

            }

        }

        public Shape FindShape(Point point)
        {
            // Поиск фигуры, находящейся под курсором (сначала проверяем последние добавленные)
            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].Contains(point))
                {
                    return _shapes[i];
                }
            }
            return null;
        }

        public void DrawAllShapes(Graphics g)
        {
            foreach (var shape in _shapes)
            {
                shape.Draw(g);
            }
        }

        // Метод для выделения нескольких фигур
        public void SelectShapes(List<Shape> shapes)
        {
            ClearSelection();
            foreach (var shape in shapes)
            {
                if (shape != null)
                {
                    shape.IsSelected = true;
                }
            }
        }

        // Метод для получения списка выделенных фигур
        public List<Shape> GetSelectedShapes()
        {
            List<Shape> selectedShapes = new List<Shape>();
            foreach (var shape in _shapes)
            {
                if (shape.IsSelected)
                {
                    selectedShapes.Add(shape);
                }
            }
            return selectedShapes;
        }

        // Метод для удаления выбранных фигур
        public void DeleteSelectedShapes()
        {
            List<Shape> selectedShapes = GetSelectedShapes();
            foreach (var shape in selectedShapes)
            {
                RemoveShape(shape);
            }
        }

        // Добавляем новый метод для очистки контейнера
        public void Clear()
        {
            _shapes.Clear();
        }
    }
}