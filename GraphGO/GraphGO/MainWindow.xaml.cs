using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphGO;
using Microsoft.Win32;
using System.IO;
using System.Windows.Navigation;


namespace GraphDemo
{
    public partial class MainWindow : Window
    {
        // Перечисления для машины состояний
        private enum Mode
        {
            Default, AddVertex, AddEdge, Move, Dijkstra, Belman, AStar, Delete  
        }
        // Текущиее состояние
        private Mode currentMode;

        // Хранение вершин и рёбер в листах
        private List<Vertex> vertices = new List<Vertex>();
        private List<Edge> edges = new List<Edge>();

        // Временные переменные для хранения первого выбранной вершины и алгоритмов
        private Vertex firstVertexForEdge = null;
        private Vertex startVertex = null;
        private Vertex endVertex = null;

        // Переменные для перемещения вершины
        private Vertex draggingVertex = null;
        private Point dragOffset;
        // Переменные вывода сообщения
        private bool showDijkstraMessage = true;
        private bool showBelmanMessage = true;
        private bool showAstarMessage = true;
        // Константа для радиуса вершины
        private const double VertexRadius = 20;

        public MainWindow()
        {
            InitializeComponent();
            SetMode(Mode.Default); // Установака дефолтного состояния
           
        }

        /// <summary>
        /// Сброс временных состояний и выделений
        /// </summary>
        private void ResetState()
        {
            // Сброс выделения для создания ребра
            if (firstVertexForEdge != null)
            {
                firstVertexForEdge.Ellipse.Stroke = Brushes.Black;
                firstVertexForEdge = null;
            }
            // Сброс выбора вершин для алгоритма Дейкстры
            if (startVertex != null)
            {
                startVertex.Ellipse.Stroke = Brushes.Black;
                startVertex = null;
            }
            if (endVertex != null)
            {
                endVertex.Ellipse.Stroke = Brushes.Black;
                endVertex = null;
            }
            draggingVertex = null;
            // Сброс подсветки рёбер (например, предыдущий путь выделен зелёным)
            foreach (var edge in edges)
            {
                edge.Line.Stroke = Brushes.Black;
                edge.Line.StrokeThickness = 2;
            }
        }

        /// <summary>
        /// Устанавливает режим работы, сбрасывая  состояния и обновляя оформление кнопок.
        /// </summary>
        /// <param name="mode">Новый режим</param>
        private void SetMode(Mode mode)
        {
            // Сброс состояний
            ResetState();
            currentMode = mode;

            // Обновление оформления кнопок
            btnAddVertex.Opacity = 1.0;
            btnAddEdge.Opacity = 1.0;
            btnMove.Opacity = 1.0;
            btnSolveAStar.Opacity = 1.0;
            btnSolveBelman.Opacity = 1.0;
            btnSolveDjikstra.Opacity = 1.0;
            btnDelete.Opacity = 1.0;

            switch (mode)
            {
                case Mode.Default:
                    // Режим по умолчанию – ни одна функция не активна
                    GraphCanvas.Cursor = Cursors.Arrow;
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: По умолчанию";
                    break;
                case Mode.AddVertex:
                    // Режим добавления вершины
                    btnAddVertex.Opacity = 0.5;
                    GraphCanvas.Cursor = Cursors.Pen;
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Добавление вершин";
                    break;
                case Mode.AddEdge:
                    // Режим добавления грани
                    btnAddEdge.Opacity = 0.5;
                    GraphCanvas.Cursor = Cursors.Cross;
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Добавление граней";
                    break;
                case Mode.Delete:
                    // Режим удаления вершины
                    btnDelete.Opacity = 0.5;
                    GraphCanvas.Cursor = Cursors.Help;
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Удаление вершин";
                    break;
                case Mode.Move:
                    // Режим перемещения вершины
                    btnMove.Opacity = 0.5;
                    GraphCanvas.Cursor = Cursors.Hand;
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Перемещение вершин";
                    break;
                case Mode.Dijkstra:
                    // Режим Дейкстры
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Режим Дейкстра";
                    GraphCanvas.Cursor = Cursors.Arrow;
                    btnSolveDjikstra.Opacity = 0.5;
                    if (showDijkstraMessage)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            "Выберите сначала начальную, затем конечную вершину для поиска кратчайшего пути.\n\n" +
                            "Нажмите 'Да', чтобы не показывать это сообщение снова.\n" +
                            "Нажмите 'Нет', чтобы видеть его при каждом входе в режим.",
                            "Aлгоритм Дейкстра",
                            MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                            showDijkstraMessage = false;
                    }
                    break;
                case Mode.Belman:
                    //  Режим Беллмана-Форда
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Режим Беллмана-Форда";
                    GraphCanvas.Cursor = Cursors.Arrow;
                    btnSolveBelman.Opacity = 0.5;
                    if (showBelmanMessage)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            "Выберите сначала начальную, затем конечную вершину для поиска кратчайшего пути.\n\n" +
                            "Нажмите 'Да', чтобы не показывать это сообщение снова.\n" +
                            "Нажмите 'Нет', чтобы видеть его при каждом входе в режим.",
                            "Алгоритм Беллмана–Форда",
                            MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                            showBelmanMessage = false;
                    }
                    break;
                case Mode.AStar:
                    //  Режим Режим A*
                    Title = "Graph GO | Программа демонстратор для решения задач на графах | Режим: Режим A*";
                    GraphCanvas.Cursor = Cursors.Arrow;
                    btnSolveAStar.Opacity = 0.5;
                    if (showAstarMessage)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            "Выберите сначала начальную, затем конечную вершину для поиска кратчайшего пути.\n\n" +
                            "Нажмите 'Да', чтобы не показывать это сообщение снова.\n" +
                            "Нажмите 'Нет', чтобы видеть его при каждом входе в режим.",
                            "Алгоритм A*",
                            MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                            showAstarMessage = false;
                    }
                    break;
            }
        }

       
        // Обработчик мажатий мыши по canvas
     
        private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GraphCanvas); // Получение координат клика

            switch (currentMode)
            {
                case Mode.Default:
                    // В режиме по умолчанию клики игнорируются.
                    
                    
                    break;
                case Mode.AddVertex: // Создание вершины
                    CreateVertex(pos); 
                    break;
                case Mode.AddEdge: // Создание грани
                    {
                        Vertex clicked = GetVertexAtPoint(pos);
                        if (clicked != null)
                        {
                            if (firstVertexForEdge == null)
                            {
                                firstVertexForEdge = clicked;
                                clicked.Ellipse.Stroke = Brushes.Red;
                            }
                            else if (firstVertexForEdge != clicked)
                            {
                                int weight = 1;
                                CreateEdge(firstVertexForEdge, clicked, weight);
                                firstVertexForEdge.Ellipse.Stroke = Brushes.Black;
                                firstVertexForEdge = null;
                            }
                        }
                    }
                    break;
                case Mode.Move: // Перемещение вершин
                    {
                        Vertex v = GetVertexAtPoint(pos);
                        if (v != null)
                        {
                            draggingVertex = v;
                            dragOffset = new Point(pos.X - v.Position.X, pos.Y - v.Position.Y);
                            
                        }
                    }
                    break;
                case Mode.Dijkstra: // Алгоритм Дейкстры
                    {
                        Vertex clicked = GetVertexAtPoint(pos); // Получение вершины по координатам
                        if (clicked != null)
                        {
                            if (startVertex == null)
                            {
                                startVertex = clicked;
                                startVertex.Ellipse.Stroke = Brushes.Blue; // Выделение первого выбранного 
                            }
                            else if (clicked == startVertex)
                            {
                                MessageBox.Show("Эта вершина уже выбрана как начальная. Выберите другую вершину.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                            else if (endVertex == null)
                            {
                                endVertex = clicked;
                                endVertex.Ellipse.Stroke = Brushes.Green;  // Выделение второго выбранного 
                                RunDijkstra(); // Запуск алгоритма Дейкстры
                            }
                        }
                    }
                    break;
                case Mode.Belman:// Алгоритм Беллмана-Форда
                    {
                        Vertex clicked = GetVertexAtPoint(pos);
                        if (clicked != null)
                        {
                            if (startVertex == null)
                            {
                                startVertex = clicked;
                                startVertex.Ellipse.Stroke = Brushes.Blue; 
                            }
                            else if (clicked == startVertex)
                            {
                                MessageBox.Show("Эта вершина уже выбрана как начальная. Выберите другую вершину.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                            else if (endVertex == null)
                            {
                                endVertex = clicked;
                                endVertex.Ellipse.Stroke = Brushes.Green; 
                                RunBelman(); // Запуска алгоритма Беллмана-Форда
                            }
                        }
                    }
                    break;
                case Mode.AStar:// Алгоритм A*
                    {
                        Vertex clicked = GetVertexAtPoint(pos);
                        if (clicked != null)
                        {
                            if (startVertex == null)
                            {
                                startVertex = clicked;
                                startVertex.Ellipse.Stroke = Brushes.Blue; 
                            }
                            else if (clicked == startVertex)
                            {
                                MessageBox.Show("Эта вершина уже выбрана как начальная. Выберите другую вершину.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                            else if (endVertex == null)
                            {
                                endVertex = clicked;
                                endVertex.Ellipse.Stroke = Brushes.Green;
                                RunAStar(); // Запуск алгоритма A*
                            }
                        }
                    }
                    break;
                case Mode.Delete: // Удаление
                    {
                        Vertex vertexToDelete = GetVertexAtPoint(pos);
                        if (vertexToDelete != null)
                        {
                            DeleteVertex(vertexToDelete); // Удаление выбранной вершины
                        }
                        
                    }
                    break;
            }
        }

       

        // Обработчик перемещения по canvas
        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(GraphCanvas);
         

            if (currentMode == Mode.Move && draggingVertex != null)
            {
                draggingVertex.Position = new Point(pos.X - dragOffset.X, pos.Y - dragOffset.Y);
                // Обновляем расположение вершины
                Canvas.SetLeft(draggingVertex.Ellipse, draggingVertex.Position.X - VertexRadius);
                Canvas.SetTop(draggingVertex.Ellipse, draggingVertex.Position.Y - VertexRadius);
                Canvas.SetLeft(draggingVertex.Label, draggingVertex.Position.X - 5);
                Canvas.SetTop(draggingVertex.Label, draggingVertex.Position.Y - 10);

                // Обновляем положение рёбер, примыкающих к данной вершине
                foreach (var edge in edges)
                {
                    if (edge.From == draggingVertex || edge.To == draggingVertex)
                    {
                        UpdateEdgeVisual(edge);
                    }
                }
            }
        }


        private void GraphCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentMode == Mode.Move && draggingVertex != null)
            {
                draggingVertex = null;
            }
        }

        /// <summary>
        /// Создает вершину на canvas
        /// </summary>
        /// <param name="pos">Координаты вершины</param>
        private void CreateVertex(Point pos)
        {
            Vertex vertex = new Vertex(); // Создание экземпляра класса Vertex

            if (vertices.Count > 0)
            {
                vertex.Id = vertices[vertices.Count - 1].Id + 1;
            }
            else
            {
                vertex.Id = 0;
            }
            vertex.Position = pos;

            Ellipse ellipse = new Ellipse // Создание круга
            {
                Width = VertexRadius * 2,
                Height = VertexRadius * 2,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            vertex.Ellipse = ellipse;

            Canvas.SetLeft(ellipse, pos.X - VertexRadius);
            Canvas.SetTop(ellipse, pos.Y - VertexRadius);

            GraphCanvas.Children.Add(ellipse); // Добавление на canvas

            TextBlock label = new TextBlock // Создание надписи которая отображает ID
            {
                Text = vertex.Id.ToString(),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                 IsHitTestVisible = false  
            };
            vertex.Label = label;
            Canvas.SetLeft(label, pos.X - 5);
            Canvas.SetTop(label, pos.Y - 10);
            GraphCanvas.Children.Add(label); // Добавление на canvas

            vertices.Add(vertex); // Добавление вершины в лист вершин
        }



        /// <summary>
        /// Создает грань между вершинами
        /// </summary>
        /// <param name="from">Первая вершина</param>
        /// <param name="to">Вторая вершина</param>
        /// <param name="weight">Вес вершины</param>
        private void CreateEdge(Vertex from, Vertex to, int weight)
        {
            Edge edge = new Edge
            {
                From = from,
                To = to,
                Weight = weight
            };

            // Вычисляем направление между центрами вершин и нормализуем вектор
            double dx = to.Position.X - from.Position.X;
            double dy = to.Position.Y - from.Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double offsetX = 0, offsetY = 0;
            if (distance > 0)
            {
                offsetX = (dx / distance) * VertexRadius;
                offsetY = (dy / distance) * VertexRadius;
            }

            // Стартовая и конечная точки линии на границе вершин
            double startX = from.Position.X + offsetX;
            double startY = from.Position.Y + offsetY;
            double endX = to.Position.X - offsetX;
            double endY = to.Position.Y - offsetY;

            Line line = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            edge.Line = line;
            GraphCanvas.Children.Add(line);

            
            TextBlock weightLabel = new TextBlock // Создание надписи которая отображает вес
            {
                Text = weight.ToString(),
                Foreground = Brushes.Red,
                Background = Brushes.White,
                Cursor = Cursors.Hand
            };
            weightLabel.Tag = edge;
            weightLabel.MouseLeftButtonDown += WeightLabel_MouseLeftButtonDown; // Привязка вызова метода по нажатию на надпись
            double midX = (startX + endX) / 2;
            double midY = (startY + endY) / 2;
            Canvas.SetLeft(weightLabel, midX);
            Canvas.SetTop(weightLabel, midY);
            edge.WeightLabel = weightLabel;
            GraphCanvas.Children.Add(weightLabel);

            edges.Add(edge); // Добавление грани в лист граней
        }
        /// <summary>
        /// Удаление выбранной вершины и связанных с ней граней
        /// </summary>
        /// <param name="from">Выбранная вершина</param>
        private void DeleteVertex(Vertex vertex)
        {
            // Получение всех граней граней, которые связаны с вершиной
            var edgesToRemove = edges.Where(edge => edge.From == vertex || edge.To == vertex).ToList();
            foreach (var edge in edgesToRemove)
            {
                if (GraphCanvas.Children.Contains(edge.Line))
                    GraphCanvas.Children.Remove(edge.Line);      // Удаление визуала для каждой грани
                if (GraphCanvas.Children.Contains(edge.WeightLabel))
                    GraphCanvas.Children.Remove(edge.WeightLabel);
                edges.Remove(edge); // Удаление грани ищ списка граней
            }

            // Удаление визуала вершины
            if (GraphCanvas.Children.Contains(vertex.Ellipse))
                GraphCanvas.Children.Remove(vertex.Ellipse);
            if (GraphCanvas.Children.Contains(vertex.Label))
                GraphCanvas.Children.Remove(vertex.Label);

            
            vertices.Remove(vertex); // Удаление вершины из списка вершин
        }
        // Обработчик изменения веса ребра
        private void WeightLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock label && label.Tag is Edge edge)
            {
                var dlg = new InputDialog(edge.Weight.ToString())
                {
                    Owner = this
                };

                if (dlg.ShowDialog() == true)
                {
                    
                    edge.Weight = dlg.ResponseText;
                    label.Text = dlg.ResponseText.ToString();
                    
                }
            }
        }

        /// <summary>
        /// Получение вершины по координатам с учетом радиуса
        /// </summary>
        /// <param name="point">Координаты</param>
        private Vertex GetVertexAtPoint(Point point)
        {
            foreach (var vertex in vertices)
            {
                double dx = vertex.Position.X - point.X;
                double dy = vertex.Position.Y - point.Y;
                if (Math.Sqrt(dx * dx + dy * dy) <= VertexRadius) // Проверка на принадлежность точки к вершине
                    return vertex;
            }
            return null;
        }


        /// <summary>
        /// Перерисовка отображения вершины
        /// </summary>
        /// <param name="edge">Вершина</param>
        private void UpdateEdgeVisual(Edge edge)
        {
            double dx = edge.To.Position.X - edge.From.Position.X;
            double dy = edge.To.Position.Y - edge.From.Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double offsetX = 0, offsetY = 0;
            if (distance > 0)
            {
                offsetX = (dx / distance) * VertexRadius;
                offsetY = (dy / distance) * VertexRadius;
            }
            double startX = edge.From.Position.X + offsetX;
            double startY = edge.From.Position.Y + offsetY;
            double endX = edge.To.Position.X - offsetX;
            double endY = edge.To.Position.Y - offsetY;
            edge.Line.X1 = startX;
            edge.Line.Y1 = startY;
            edge.Line.X2 = endX;
            edge.Line.Y2 = endY;
            double midX = (startX + endX) / 2;
            double midY = (startY + endY) / 2;
            Canvas.SetLeft(edge.WeightLabel, midX);
            Canvas.SetTop(edge.WeightLabel, midY);
        }

        // Запуск алгоритма Дейкстры после выбора двух вершин
        private void RunDijkstra()
        {
            var prev = Dijkstra(startVertex.Id, endVertex.Id);
            if (prev == null)
            {
                MessageBox.Show("Путь не найден.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ResetState();
            }
            else
            {
                HighlightPath(prev); // Подсветка найденного пути по полученному словарю
            }
            
        }
        // Запуск алгоритма Беллмана-Форда после выбора двух вершин
        private void RunBelman()
        {
            var prev = BellmanFord(startVertex.Id, endVertex.Id);
            if (prev == null)
            {
                MessageBox.Show("Путь не найден.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ResetState();
            }
            else
            {
                HighlightPath(prev);
            }

        }
        // Запуск алгоритма A* после выбора двух вершин
        private void RunAStar()
        {
            var prev = AStar(startVertex.Id, endVertex.Id);
            if (prev == null)
            {
                MessageBox.Show("Путь не найден.", "Предупреждение",MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ResetState();
            }
            else
            {
                HighlightPath(prev);
            }


        }




        /// <summary>
        /// Алгоритм Дейкстры
        /// </summary>
        /// <param name="startId">ID первой вершины</param>
        /// <param name="startId">ID второй вершины</param>
        private Dictionary<int, int> Dijkstra(int startId, int endId)
        {
            // Проверка: если обнаружены отрицательные веса, выводим сообщение и прекращаем выполнение алгоритма
            if (edges.Any(edge => edge.Weight < 0))
            {
                MessageBox.Show("Ошибка: обнаружены отрицательные веса. Алгоритм Дейкстры не работает для графов с отрицательными весами.");
                return null;
            }

            Dictionary<int, int> dist = new Dictionary<int, int>();
            Dictionary<int, int> prev = new Dictionary<int, int>();

            foreach (var vertex in vertices)
            {
                dist[vertex.Id] = int.MaxValue;
                prev[vertex.Id] = -1;
            }
            dist[startId] = 0;

            //  Множество для хранения непосещенных вершин
            var unvisited = new HashSet<int>(vertices.Select(v => v.Id));

            while (unvisited.Count > 0)
            {
                int current = unvisited.OrderBy(id => dist[id]).First();
                if (dist[current] == int.MaxValue)
                    break;
                unvisited.Remove(current);
                if (current == endId)
                    break;

                foreach (var edge in edges.Where(e => e.From.Id == current || e.To.Id == current))
                {
                    int neighbor = 0;
                    if (edge.From.Id == current) {
                        neighbor = edge.To.Id;
                    } else 
                    {
                        neighbor = edge.From.Id;
                    }
                    if (!unvisited.Contains(neighbor))
                        continue;

                    int alt = dist[current] + edge.Weight;
                    if (alt < dist[neighbor])
                    {
                        dist[neighbor] = alt;
                        prev[neighbor] = current;
                    }
                }
            }
            if (dist[endId] == int.MaxValue)
            {
                return null;
            }
            else
            {
                return prev;
            }
        }
        /// <summary>
        /// Алгоритм Беллмана-Форда
        /// </summary>
        /// <param name="startId">ID первой вершины</param>
        /// <param name="startId">ID второй вершины</param>
        private Dictionary<int, int> BellmanFord(int startId, int endId)
        {
      
            Dictionary<int, int> dist = new Dictionary<int, int>();
            Dictionary<int, int> prev = new Dictionary<int, int>();

            foreach (var vertex in vertices)
            {
                dist[vertex.Id] = int.MaxValue;
                prev[vertex.Id] = -1;
            }
            dist[startId] = 0;

            int V = vertices.Count;

            // Релаксируем все рёбра (V-1) раз
            for (int i = 1; i < V; i++)
            {
                foreach (var edge in edges)
                {
                    int u = edge.From.Id;
                    int v = edge.To.Id;
                    // Релаксируем ребро в направлении u -> v
                    if (dist[u] != int.MaxValue && dist[u] + edge.Weight < dist[v])
                    {
                        dist[v] = dist[u] + edge.Weight;
                        prev[v] = u;
                    }
                    // Обратная релаксация
                    if (dist[v] != int.MaxValue && dist[v] + edge.Weight < dist[u])
                    {
                        dist[u] = dist[v] + edge.Weight;
                        prev[u] = v;
                    }
                }
            }

            //  Проверка на отрицательные циклы.
            foreach (var edge in edges)
            {
                int u = edge.From.Id, v = edge.To.Id;
                if (dist[u] != int.MaxValue && dist[u] + edge.Weight < dist[v])
                {
                    MessageBox.Show("Обнаружены отрицательные циклы", "Предупреждение");
                    return null;
                }

                if (dist[v] != int.MaxValue && dist[v] + edge.Weight < dist[u])
                {
                    MessageBox.Show("Обнаружены отрицательные циклы", "Предупреждение");
                    return null;

                }
            }

            if (dist[endId] == int.MaxValue)
                return null;
            return prev;
        }
        /// <summary>
        /// Алгоритм A*
        /// </summary>
        /// <param name="startId">ID первой вершины</param>
        /// <param name="startId">ID второй вершины</param>
        private Dictionary<int, int> AStar(int startId, int endId)
        {
            // Инициализируем gScore, fScore и prev для всех вершин.
            var gScore = vertices.ToDictionary(v => v.Id, _ => double.PositiveInfinity);
            var fScore = vertices.ToDictionary(v => v.Id, _ => double.PositiveInfinity);
            var prev = vertices.ToDictionary(v => v.Id, _ => -1);

   
            gScore[startId] = 0;
            fScore[startId] = Heuristic(startId, endId);

          

            var openSet = new List<int> { startId };
            var closedSet = new HashSet<int>();

            while (openSet.Count > 0)
            {
                // Выбираем вершину с минимальным fScore.
                int current = openSet.OrderBy(id => fScore[id]).First();
             

                // Если достигли цели – возвращаем словарь предшественников.
                if (current == endId)
                {
               
                    return prev;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                // Обходим всех соседей текущей вершины через прилегающие ребра.
                foreach (var edge in edges.Where(e => e.From.Id == current || e.To.Id == current))
                {
                    int neighbor = 0;
                    if (edge.From.Id == current) {
                        neighbor = edge.To.Id;
                    } else {
                        neighbor = edge.From.Id;
                    }
                    if (closedSet.Contains(neighbor))
                        continue;

                    double tentativeG = gScore[current] + edge.Weight;
              

                    if (tentativeG < gScore[neighbor])
                    {
                        gScore[neighbor] = tentativeG;
                        double heuristicValue = Heuristic(neighbor, endId);
                        fScore[neighbor] = tentativeG + heuristicValue;
                        prev[neighbor] = current;
                       
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                         }
                    }
                }
                
            }

            return null;
        }



        /// <summary>
        /// Подсветка найденного пути по словарю
        /// </summary>
        /// <param name="prev">Словарь</param>

        private void HighlightPath(Dictionary<int, int> prev)
        {
            // Сбросим все рёбра в стандартное оформление
            foreach (var edge in edges)
            {
                edge.Line.Stroke = Brushes.Black;
                edge.Line.StrokeThickness = 2;
            }
            int current = endVertex.Id;
            while (prev[current] != -1)
            {
                int fromId = prev[current];
                int toId = current;
                foreach (var edge in edges)
                {
                    if ((edge.From.Id == fromId && edge.To.Id == toId) ||
                        (edge.From.Id == toId && edge.To.Id == fromId))
                    {
                        edge.Line.Stroke = Brushes.Green;
                        edge.Line.StrokeThickness = 3;
                    }
                }
                current = fromId;
            }
            // Сброс выделения выбранных вершин
            if (startVertex != null)
                startVertex.Ellipse.Stroke = Brushes.Black;
            if (endVertex != null)
                endVertex.Ellipse.Stroke = Brushes.Black;
            startVertex = null;
            endVertex = null;
        }

        /// <summary>
        /// Экспорт конфигурации графа в JSON
        /// </summary>
        /// <param name="filename">Путь до файла</param>

        private void ExportGraph(string filename)
        {
            GraphData graphData = new GraphData();

            // Заполняем список вершин
            foreach (var vertex in vertices) // vertices – лист объектов Vertex
            {
                graphData.Vertices.Add(new VertexData
                {
                    Id = vertex.Id,
                    X = vertex.Position.X,
                    Y = vertex.Position.Y
                });
            }

            // Заполняем список рёбер
            foreach (var edge in edges) // edges – лист объектов Edge
            {
                graphData.Edges.Add(new EdgeData
                {
                    From = edge.From.Id,
                    To = edge.To.Id,
                    Weight = edge.Weight
                });
            }

            // Создание JSON с использованием Newtonsoft.Json
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(graphData, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(filename, json);
        }


        /// <summary>
        /// Импорт конфигурации графа в JSON
        /// </summary>
        /// <param name="filename">Путь до файла</param>

        private void ImportGraph(string filename)
        {
            // Считываем JSON из файла
            string json = System.IO.File.ReadAllText(filename);
            GraphData graphData = Newtonsoft.Json.JsonConvert.DeserializeObject<GraphData>(json);

            // Очищаем Canvas и коллекции
            GraphCanvas.Children.Clear();
            vertices.Clear();
            edges.Clear();

            // Восстанавливаем вершины
            foreach (VertexData vd in graphData.Vertices)
            {
                // Создаём объект Vertex и его визуальное представление
                Vertex vertex = new Vertex
                {
                    Id = vd.Id,
                    Position = new Point(vd.X, vd.Y)
                };

                // Создаём эллипс для вершины
                Ellipse ellipse = new Ellipse
                {
                    Width = VertexRadius * 2,
                    Height = VertexRadius * 2,
                    Fill = Brushes.LightBlue,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                vertex.Ellipse = ellipse;
                Canvas.SetLeft(ellipse, vertex.Position.X - VertexRadius);
                Canvas.SetTop(ellipse, vertex.Position.Y - VertexRadius);
                GraphCanvas.Children.Add(ellipse);

                // Создаём метку
                TextBlock label = new TextBlock
                {
                    Text = vertex.Id.ToString(),
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false // чтобы клики проходили к Ellipse
                };
                vertex.Label = label;
                Canvas.SetLeft(label, vertex.Position.X - 5);
                Canvas.SetTop(label, vertex.Position.Y - 10);
                GraphCanvas.Children.Add(label);

                // Добавляем вершину в коллекцию
                vertices.Add(vertex);
            }

            // Восстанавливаем рёбра
            foreach (EdgeData ed in graphData.Edges)
            {
                // Находим объекты Vertex по ID
                Vertex from = vertices.FirstOrDefault(v => v.Id == ed.From);
                Vertex to = vertices.FirstOrDefault(v => v.Id == ed.To);
                if (ed.Weight < 1) // Проверка на отрицательные пути
                {
                    MessageBox.Show("Обнаружены отрицательные веса. Импорт остановлен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    GraphCanvas.Children.Clear();
                    vertices.Clear();
                    edges.Clear();
                    return;
                }
                if (from != null && to != null)
                {
                    // Создание ребра
                    CreateEdge(from, to, ed.Weight);
                    
                    
                }
            }
        }
        
        // Обработчик кнопки импорта
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json"
            };

            bool? result = dlg.ShowDialog();
            // Проверка, что файл имеет правильное расширение
            if (System.IO.Path.GetExtension(dlg.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                ImportGraph(dlg.FileName);
                if (vertices.Count > 0) {
                    MessageBox.Show($"Граф импортирован из файла: {dlg.FileName}", "Импорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выбранный файл не является форматом JSON.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        // Обработчик кнопки экспорт
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "data",
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                ExportGraph(filename);
                MessageBox.Show($"Граф экспортирован в файл: {filename}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Обработчик кнопки выход
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Метод расчета эвристики
        private double Heuristic(int vertexId, int goalId)
        {
            Vertex vertex = vertices.First(vtx => vtx.Id == vertexId);
            Vertex goal = vertices.First(g => g.Id == goalId);
            double dx = vertex.Position.X - goal.Position.X;
            double dy = vertex.Position.Y - goal.Position.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }


      
        /// <summary>
        /// Выполняет обход графа в ширину (BFS).
        /// </summary>

        private void BreadthFirstSearch()
        {
            int startId = 0;
            // Множество посещённых вершин
            var visited = new HashSet<int>();
            // Список для хранения порядка обхода
            var order = new List<int>();
            // Очередь для BFS
            var queue = new Queue<int>();

            // Начинаем с вершины startId
            queue.Enqueue(startId);
            visited.Add(startId);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                order.Add(currentId);

                // Для каждой вершины, смежной с currentId,
                // ищем соседей через просмотр каждого ребра.
                foreach (var edge in edges)
                {
                    int neighborId = -1;
                    if (edge.From.Id == currentId)
                    {
                        neighborId = edge.To.Id;
                    }
                    else if (edge.To.Id == currentId)
                    {
                        neighborId = edge.From.Id;
                    }

                    // Если сосед обнаружен и не был посещён, добавляем его в очередь
                    if (neighborId != -1 && !visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }

            MessageBox.Show("Порядок обхода (BFS): " + string.Join(" -> ", order),"Поиск в ширину", MessageBoxButton.OK, MessageBoxImage.Information);
            AnimateBFS(order);

          
        }

        /// <summary>
        /// Выполняет обход графа в глубину (DFS)
        /// </summary>
        private List<int> DepthFirstSearch(int startId)
        {
            var visited = new HashSet<int>();
            var order = new List<int>();
            DFSRecursive(startId, visited, order);
            return order;
        }

        /// <summary>
        /// Рекурсивная вспомогательная функция для DFS.
        /// </summary>
        private void DFSRecursive(int currentId, HashSet<int> visited, List<int> order)
        {
            visited.Add(currentId);
            order.Add(currentId);

            // Просматриваем все ребра, чтобы найти соседей текущей вершины
            foreach (var edge in edges)
            {
                int neighborId = -1;
                if (edge.From.Id == currentId)
                {
                    neighborId = edge.To.Id;
                }
                else if (edge.To.Id == currentId)
                {
                    neighborId = edge.From.Id;
                }

                // Если сосед не посещён, продолжаем рекурсивный обход
                if (neighborId != -1 && !visited.Contains(neighborId))
                {
                    DFSRecursive(neighborId, visited, order);
                }
            }
        }


        // Метод для анимации
        private async void AnimateBFS(List<int> order)
        {
            foreach (int vertexId in order)
            {
                HighlightVertex(vertexId);
                await Task.Delay(500);
                UnhighlightVertex(vertexId);
            }
        }

        // Обработчик кнопки поиск в ширину
        private void btnBFS_Click(object sender, RoutedEventArgs e)
        {
            if (edges.Count != 0 )
            {
                BreadthFirstSearch();
            } else
            {
                MessageBox.Show("Добавьте грани", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
                
        }

        /// <summary>
        /// Метод применения подсветки выбранной вершины
        /// </summary>
        /// <param name="vertexId">ID вершины</param>
        private void HighlightVertex(int vertexId)
        {
            Vertex vertex = vertices.FirstOrDefault(v => v.Id == vertexId);
            if (vertex != null && vertex.Ellipse != null)
            {
               
                vertex.Ellipse.Stroke = Brushes.Red;
                vertex.Ellipse.StrokeThickness = 4;
            }
        }

        /// <summary>
        /// Метод сброса подсветки выбранной вершины
        /// </summary>
        /// <param name="vertexId">ID вершины</param>
        private void UnhighlightVertex(int vertexId)
        {
        
            Vertex vertex = vertices.FirstOrDefault(v => v.Id == vertexId);
            if (vertex != null && vertex.Ellipse != null)
            {
                
                vertex.Ellipse.Stroke = Brushes.Black;
                vertex.Ellipse.StrokeThickness = 2;
            }
        }

        // Метод для запуска поиска в глубину
        private async void StartDFS()
        {
            
            int startId = 0;

            
            List<int> dfsOrder = DepthFirstSearch(startId);

            
            string orderText = string.Join(" -> ", dfsOrder);
            MessageBox.Show("Порядок обхода (DFS): " + orderText, "Поиск в глубину", MessageBoxButton.OK,MessageBoxImage.Information);

            // Анимация: подсвечиваем вершины по очереди
            foreach (int vertexId in dfsOrder)
            {
                HighlightVertex(vertexId);
                await Task.Delay(500);
                UnhighlightVertex(vertexId);
            }
        }
        // Обработчик кнопки поиск в глубину
        private void btnDFS_Click(object sender, RoutedEventArgs e)
        {
            if (edges.Count != 0)
            {
                StartDFS();
            }
            else
            {
                MessageBox.Show("Добавьте грани", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обработчик кнопки добавить вершину
        private void btnAddVertex_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetMode(Mode.AddVertex);
        }

        // Обработчик кнопки добавить грань
        private void btnAddEdge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetMode(Mode.AddEdge);
        }

        // Обработчик кнопки перемещение вершин
        private void btnMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetMode(Mode.Move);
        }
        // Обработчик кнопки удаления вершин
        private void btnDelete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetMode(Mode.Delete);
   
        }
        // Обработчик кнопки очистки графа
        private void btnClear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GraphCanvas.Children.Clear();
            vertices.Clear();
            edges.Clear();
            ResetState();
        }



        // Обработчик кнопки алгоритм Дейкстра

        private void btnSolveDjikstra_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.Dijkstra);
        }

        // Обработчик кнопки алгоритм Беллмана-Форда
        private void btnSolveBelman_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.Belman);
        }

        // Обработчик кнопки алгоритм A*
        private void btnSolveAStar_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.AStar);
        }

        // Обработчик кнопки добавить вершину из меню
        private void btnAddVertexMenu_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.AddVertex);
        }
        // Обработчик кнопки добавить грань из меню
        private void btbAddEdgeMenu_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.AddEdge);
        }


        // Обработчик кнопки удаления вершин из меню
        private void btbDeleteVertexMenu_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.Delete);
        }

        // Обработчик кнопки очистки графа из меню
        private void btbClearMenu_Click(object sender, RoutedEventArgs e)
        {
            GraphCanvas.Children.Clear();
            vertices.Clear();
            edges.Clear();
            ResetState();
        }

        // Обработчик кнопки алгоритм Дейкстра из меню

        private void btnSolveDijkstraMenu_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.Dijkstra);
        }

        // Обработчик кнопки алгоритм Беллмана-Форда из меню
        private void btnSolveBelmanMenu_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.Belman);
        }

        // Обработчик кнопки алгоритм A* из меню

        // Обработчик кнопки алгоритм поиск в ширину из меню
        private void btnSolveBFS_Click(object sender, RoutedEventArgs e)
        {
            if (edges.Count != 0)
            {
                BreadthFirstSearch();
            }
            else
            {
                MessageBox.Show("Добавьте грани", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обработчик кнопки алгоритм поиск в глубину из меню
        private void btnSolveDFS_Click(object sender, RoutedEventArgs e)
        {
            if (edges.Count != 0)
            {
                StartDFS();
            }
            else
            {
                MessageBox.Show("Добавьте грани", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Обработчик кнопки о программе
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutProgram aboutProgram = new AboutProgram();
            aboutProgram.ShowDialog();
        }

        // Обработчик кнопки алгоритм A* из меню
        private void btnSolveAStarMenu_Click(object sender, RoutedEventArgs e)
        {
            SetMode(Mode.AStar);
        }
    }




  


    
  
}
