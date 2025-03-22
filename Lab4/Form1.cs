using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab4
{
    

    public partial class Form1 : Form
    {
        private readonly ShapeContainer _shapeContainer = new ShapeContainer();
        private Shape _selectedShape; // Текущая выбранная фигура
        private Point _mouseDownLocation; // Точка, где была нажата мышь
        private bool _isDragging; // Флаг, указывающий на процесс перетаскивания
        private ShapeType _selectedShapeType = ShapeType.Circle; // Тип фигуры, выбранный в панели инструментов
        private readonly ColorDialog _colorDialog = new ColorDialog();
        private const int ShapeSize = 50;  // Default size for new shapes

        // Перечисление для типов фигур
        public enum ShapeType
        {
            Circle,
            Rectangle,
            Ellipse,
            Triangle,
            Line
        }
        public Form1()
        {
            InitializeComponent();
            // Настройка панели инструментов (можно добавить кнопки для выбора типов фигур)
            AddShapeTypeButtons();
            DoubleBuffered = true; // Для устранения мерцания
        }
        // Добавление кнопок для выбора типов фигур на панель инструментов
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

            ToolStripButton triangleButton = new ToolStripButton("Треугольник");
            triangleButton.Tag = ShapeType.Triangle;
            triangleButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(triangleButton);

            ToolStripButton lineButton = new ToolStripButton("Линия");
            lineButton.Tag = ShapeType.Line;
            lineButton.Click += ShapeTypeButton_Click;
            toolStrip1.Items.Add(lineButton);

            toolStrip1.Items.Add(new ToolStripSeparator());

            ToolStripButton colorButton = new ToolStripButton("Цвет");
            colorButton.Click += ChangeColorButton_Click;
            toolStrip1.Items.Add(colorButton);


            ToolStripButton deleteButton = new ToolStripButton("Удалить");
            deleteButton.Click += DeleteButton_Click;
            toolStrip1.Items.Add(deleteButton);
        }


        private void ShapeTypeButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripButton button && button.Tag is ShapeType type)
            {
                _selectedShapeType = type;
            }
        }

        // Обработчик события для кнопки "Удалить"
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            _shapeContainer.DeleteSelectedShapes();
            Invalidate(); // Перерисовываем рабочую область
        }

        private void ChangeColorButton_Click(object sender, EventArgs e)
        {
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Получаем выбранный цвет
                Color selectedColor = _colorDialog.Color;

                // Изменяем цвет выбранных фигур
                foreach (Shape shape in _shapeContainer.GetSelectedShapes())
                {
                    shape.Color = selectedColor;
                }
                Invalidate(); // Перерисовываем рабочую область
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            _shapeContainer.DrawAllShapes(e.Graphics);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseDownLocation = e.Location;

                // Проверяем, была ли нажата кнопка мыши на фигуре
                _selectedShape = _shapeContainer.FindShape(e.Location);

                // Если фигура найдена, выбираем ее
                _shapeContainer.SelectShape(_selectedShape);
                _isDragging = _selectedShape != null; // Разрешаем перетаскивание только если фигура выбрана
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedShape != null)
            {
                // Вычисляем смещение мыши
                int dx = e.X - _mouseDownLocation.X;
                int dy = e.Y - _mouseDownLocation.Y;

                // Перемещаем выбранную фигуру
                _selectedShape.Move(dx, dy, ClientRectangle);

                // Обновляем координаты мыши
                _mouseDownLocation = e.Location;

                // Перерисовываем рабочую область
                Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;  // Прекращаем перетаскивание
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (_selectedShape != null)
            {
                int moveStep = 5; // Шаг перемещения
                int resizeStep = 5; // Шаг изменения размера

                switch (e.KeyCode)
                {
                    case Keys.Up:
                        _selectedShape.Move(0, -moveStep, ClientRectangle);
                        break;
                    case Keys.Down:
                        _selectedShape.Move(0, moveStep, ClientRectangle);
                        break;
                    case Keys.Left:
                        _selectedShape.Move(-moveStep, 0, ClientRectangle);
                        break;
                    case Keys.Right:
                        _selectedShape.Move(moveStep, 0, ClientRectangle);
                        break;
                    case Keys.Add: // Увеличение размера ( "+" )
                        _selectedShape.Resize(resizeStep, resizeStep, ClientRectangle);
                        break;
                    case Keys.Subtract: // Уменьшение размера ( "-" )
                        _selectedShape.Resize(-resizeStep, -resizeStep, ClientRectangle);
                        break;
                    case Keys.Delete: // Удаление выбранного объекта
                        _shapeContainer.RemoveShape(_selectedShape);
                        _selectedShape = null;
                        break;
                }

                Invalidate(); // Перерисовываем рабочую область после перемещения/изменения размера/удаления
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Если ничего не было выбрано, то создаем новую фигуру
                if (_selectedShape == null)
                {
                    CreateShape(e.Location);
                }
                else
                {
                    // Если фигура уже выбрана, то ничего не делаем (или можно добавить функционал для отмены выбора)
                }
                Invalidate(); // Перерисовываем рабочую область
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // Перерисовываем все фигуры при изменении размеров формы
            Invalidate();
        }
        // Метод для создания новой фигуры
        private void CreateShape(Point location)
        {
            Color shapeColor = Color.Blue; // Default color
            switch (_selectedShapeType)
            {
                case ShapeType.Circle:
                    _shapeContainer.AddShape(new Circle(shapeColor, location, ShapeSize / 2));
                    break;
                case ShapeType.Rectangle:
                    _shapeContainer.AddShape(new RectangleShape(shapeColor, location, ShapeSize, ShapeSize));
                    break;
                case ShapeType.Ellipse:
                    _shapeContainer.AddShape(new Ellipse(shapeColor, location, ShapeSize, ShapeSize / 2));
                    break;
                case ShapeType.Triangle:
                    _shapeContainer.AddShape(new Triangle(shapeColor, location, ShapeSize, ShapeSize));
                    break;
                case ShapeType.Line:
                    _shapeContainer.AddShape(new Line(shapeColor, location, new Point(location.X + ShapeSize, location.Y + ShapeSize)));
                    break;
            }
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


        // Метод для перемещения фигуры
        public virtual void Move(int dx, int dy, Rectangle clientRectangle)
        {
            // Проверяем, чтобы фигура не вышла за границы рабочей области
            Point newLocation = new Point(Location.X + dx, Location.Y + dy);
            if (IsWithinBounds(newLocation, GetBounds(), clientRectangle))
            {
                Location = newLocation;
            }
        }

        // Метод для изменения размера (должен быть переопределен)
        public abstract void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle);


        // Метод для получения границ фигуры (должен быть переопределен)
        public abstract Rectangle GetBounds();


        // Вспомогательный метод для проверки выхода за границы (используется в Move и Resize)
        protected bool IsWithinBounds(Point newLocation, Rectangle bounds, Rectangle clientRectangle)
        {
            Rectangle newBounds = new Rectangle(newLocation.X, newLocation.Y, bounds.Width, bounds.Height);
            return clientRectangle.Contains(newBounds);
        }

    }

    // Класс для круга
    public class Circle : Shape
    {
        public int Radius { get; set; }

        public Circle(Color color, Point location, int radius) : base(color, location)
        {
            Radius = radius;
        }

        public override void Draw(Graphics g)
        {
            Brush brush = new SolidBrush(Color);
            g.FillEllipse(brush, Location.X - Radius, Location.Y - Radius, 2 * Radius, 2 * Radius);

            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2); // Рамка для выделения
                g.DrawEllipse(selectionPen, Location.X - Radius, Location.Y - Radius, 2 * Radius, 2 * Radius);
            }
        }

        public override bool Contains(Point point)
        {
            double dx = point.X - Location.X;
            double dy = point.Y - Location.Y;
            return dx * dx + dy * dy <= Radius * Radius;
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            //Изменяем только радиус, чтобы не было искажений круга
            int newRadius = Radius + deltaWidth / 2; //или deltaHeight/2, т.к. deltaWidth = deltaHeight
            if (newRadius > 0)
            {
                Rectangle bounds = new Rectangle(Location.X - newRadius, Location.Y - newRadius, 2 * newRadius, 2 * newRadius);
                if (clientRectangle.Contains(bounds))
                {
                    Radius = newRadius;
                }
            }

        }


        public override Rectangle GetBounds()
        {
            return new Rectangle(Location.X - Radius, Location.Y - Radius, 2 * Radius, 2 * Radius);
        }
    }

    // Класс для прямоугольника
    public class RectangleShape : Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public RectangleShape(Color color, Point location, int width, int height) : base(color, location)
        {
            Width = width;
            Height = height;
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
                Rectangle bounds = new Rectangle(Location.X, Location.Y, newWidth, newHeight);
                if (clientRectangle.Contains(bounds))
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

        public Ellipse(Color color, Point location, int width, int height) : base(color, location)
        {
            Width = width;
            Height = height;
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
                Rectangle bounds = new Rectangle(Location.X, Location.Y, newWidth, newHeight);
                if (clientRectangle.Contains(bounds))
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

    // Класс для треугольника
    public class Triangle : Shape
    {
        public int Base { get; set; }
        public int Height { get; set; }

        public Triangle(Color color, Point location, int @base, int height) : base(color, location)
        {
            Base = @base;
            Height = height;
        }

        public override void Draw(Graphics g)
        {
            Point[] points = {
                new Point(Location.X, Location.Y + Height),
                new Point(Location.X + Base / 2, Location.Y),
                new Point(Location.X + Base, Location.Y + Height)
            };

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
            // Используем барицентрические координаты для определения попадания точки внутрь треугольника
            double area = 0.5 * (-Base / 2 * Height + 0 * 0 + Base * Height);
            double s = 1 / (2 * area) * (Height * (point.X - Location.X - Base / 2) + (-Base / 2) * (point.Y - Location.Y));
            double t = 1 / (2 * area) * (Base * (point.Y - Location.Y) - 0 * (point.X - Location.X - Base / 2));
            return s >= 0 && t >= 0 && s + t <= 1;
        }

        public override void Resize(int deltaWidth, int deltaHeight, Rectangle clientRectangle)
        {
            int newBase = Base + deltaWidth;
            int newHeight = Height + deltaHeight;

            if (newBase > 0 && newHeight > 0)
            {
                // Рассчитываем границы треугольника
                int x1 = Location.X;
                int y1 = Location.Y + newHeight;
                int x2 = Location.X + newBase / 2;
                int y2 = Location.Y;
                int x3 = Location.X + newBase;
                int y3 = Location.Y + newHeight;

                Rectangle bounds = new Rectangle(Math.Min(x1, Math.Min(x2, x3)),
                                             Math.Min(y1, Math.Min(y2, y3)),
                                             Math.Abs(x1 - x3),
                                             Math.Abs(y1 - y2)); // Предполагаем, что y2 всегда меньше y1

                if (clientRectangle.Contains(bounds))
                {
                    Base = newBase;
                    Height = newHeight;
                }
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(Location.X, Location.Y, Base, Height);
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

        public override void Draw(Graphics g)
        {
            Pen pen = new Pen(Color, 2); // Толщина линии
            g.DrawLine(pen, Location, EndPoint);
            if (IsSelected)
            {
                Pen selectionPen = new Pen(Color.Black, 2);
                g.DrawLine(selectionPen, Location, EndPoint);
            }
        }

        public override bool Contains(Point point)
        {
            // Простая проверка попадания в область вокруг линии (можно улучшить)
            double distance = DistancePointToLine(point, Location, EndPoint);
            return distance <= 3; // Допустимая погрешность
        }

        // Расчет расстояния от точки до линии
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
            // Изменение размеров линии через изменение конечной точки.
            int newX = EndPoint.X + deltaWidth;
            int newY = EndPoint.Y + deltaHeight;

            // Проверка на выход за границы
            if (newX >= 0 && newX <= clientRectangle.Width && newY >= 0 && newY <= clientRectangle.Height)
            {
                EndPoint = new Point(newX, newY);
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

        public override void Move(int dx, int dy, Rectangle clientRectangle)
        {
            // Проверяем, чтобы вся линия не вышла за границы
            Point newStart = new Point(Location.X + dx, Location.Y + dy);
            Point newEnd = new Point(EndPoint.X + dx, EndPoint.Y + dy);

            Rectangle bounds = new Rectangle(Math.Min(newStart.X, newEnd.X), Math.Min(newStart.Y, newEnd.Y),
                                                Math.Abs(newStart.X - newEnd.X), Math.Abs(newStart.Y - newEnd.Y));
            if (clientRectangle.Contains(bounds))
            {
                Location = newStart;
                EndPoint = newEnd;
            }
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

        // Метод для получения списка выбранных фигур
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
    }
}
