/*The MIT License(MIT)
*Copyright(c) 2016 Tommy Liu
*
*Permission is hereby granted, free of charge, to any person obtaining a copy 
* of this software and associated documentation files (the "Software"), to 
* deal in the Software without restriction, including without limitation the 
* rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
* sell copies of the Software, and to permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
*The above copyright notice and this permission notice shall be included 
* in all copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
* THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ThompsonsMathGrapher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Variable Declarations
        private Random rand = new Random(); //random variable
        private string[] data; //this string contains every line of the input .csv file
        private List<string> columnHeaders = new List<string>();
        private double waster; //temp variable to be used and wasted
        private List<string> groupByCategories = new List<string>(); //categories and columns to group data by       
        private int distanceFromLeft = 40; //initial distance from left side of canvas to draw from
        private int widthOfTheGraphBars = 15; //the width of every bar 
        private List<Person> listOfAllPeople = new List<Person>();
        private Dictionary<string, Color> ColorDictionary = new Dictionary<string, Color>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            FontFamily = new FontFamily("Verdana");

            wiidth1.Content = "15"; //sets possible width options
            wiidth2.Content = "20";
            wiidth3.Content = "25";
            wiidth4.Content = "30";
            wiidth5.Content = "35";
            wiidth6.Content = "40";
        }

        /// <summary>
        /// this button opens the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitProgram_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Loading/Getting Data
        /// <summary>
        /// reads file into variables, 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openfiledialog1 = new Microsoft.Win32.OpenFileDialog();
            openfiledialog1.Filter = "CSV Files (*.csv)|*.csv|All Files - DO NOT USE THIS EVER|*.*";
            bool? userClickedOK = openfiledialog1.ShowDialog();

            if (userClickedOK == true)
            {
                data = File.ReadAllLines(openfiledialog1.FileName);
                PopulateListView(data); //populates the listview with rows and columns of data
                Title += $" - {openfiledialog1.FileName}";
                foreach (var item in data[0].Split(','))
                {
                    columnHeaders.Add(item);
                }
                LoadPersonsObjects(); //loads people into the listofallpeople

                try
                {
                    foreach (var item in columnHeaders)
                    {
                        ColorDictionary.Add(item.ToString(), Color.FromArgb(100, (byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256)));
                    } //assigns a color to each columnheader value
                }
                catch { }
            }
        }

        /// <summary>
        /// filles the listview with stuff from the imported data
        /// </summary>
        /// <param name="dat"></param>
        private void PopulateListView(string[] dat)
        {
            foreach (var item in dat[0].Split(','))
            {
                ListView1.Columns.Add(item); //adds the columnheaders/columns to the grip
            }

            List<string> headersExcludedData = data.Skip(1).ToList<string>(); //skips the first line which contain columnheaders
            foreach (var item in headersExcludedData)
            {
                ListView1.View = View.Details; //allows for items to be viewed inside the listview

                System.Windows.Forms.ListViewItem new1 = new System.Windows.Forms.ListViewItem(item.Split(','));

                ListView1.Items.Add(new1);
            }
        }
        #endregion
        #region Logic for Drawing of graph
        private void DrawGraphButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> groupByColumns = new List<string>();
            groupByCategories.Clear();
            col2.Width = new GridLength(0, GridUnitType.Pixel);
            col3.Width = new GridLength(1, GridUnitType.Star);

            System.Collections.IList groupResultsListboxSelection = GroupResultsListbox.SelectedItems;
            System.Collections.IList groupByListboxSelection = GroupByListBox.SelectedItems;

            foreach (var item in groupByListboxSelection)
            {
                groupByCategories.Add(item.ToString());
            }

            foreach (var item in groupResultsListboxSelection)
            {
                groupByColumns.Add(item.ToString());
            }

            if (groupByColumns.Count == 0)
            {
                groupByColumns = null;
            }

            if (WidthOfBarsComboBox.SelectedItem != null)
            {
                ComboBoxItem typeItem = (ComboBoxItem)WidthOfBarsComboBox.SelectedItem;
                string value = typeItem.Content.ToString();

                int temp = 15;
                int.TryParse(value, out temp);
                widthOfTheGraphBars = temp;
            }
            else { }

            drawXandYAxis();
            foreach (var item in groupByCategories)
            {
                List<int> dataToDraw = FilterGroupingsIntoDataToGraph(item, groupByColumns);
                if (dataToDraw == null)
                {
                    break;
                }
                if (groupByColumns == null)
                {
                    DrawABarChart(dataToDraw, columnHeaders, labelsForBars(item, groupByColumns));
                }
                else
                {
                    DrawABarChart(dataToDraw, groupByColumns, labelsForBars(item, groupByColumns));
                }
            }

            GroupByListBox.SelectedIndex = -1;
        }
       
        private List<string> labelsForBars(string item, List<string> columns)
        {
            List<string> headers = new List<string>();
            List<Person> tempListOfPeople = listOfAllPeople.Where(p => p.Data.ContainsValue(item)).ToList<Person>();

            if (columns == null)
            {
                foreach (var ite in columnHeaders)
                {
                    headers.Add(tempListOfPeople[0].Data["TeacherBand"]);
                }
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    headers.Add(tempListOfPeople[0].Data["TeacherBand"]);
                }
            }
            return headers;
        }


        private List<int> FilterGroupingsIntoDataToGraph(string item, List<string> columns)
        {
            try
            {
                List<int> DataToGraph = new List<int>();

                List<Person> filteredList = listOfAllPeople.Where(p => p.Data.ContainsValue(item) || p.Data.ContainsKey(item)).ToList<Person>();
                if (columns == null)
                {
                    for (int i = 0; i < columnHeaders.Count; i++)
                    {
                        DataToGraph.Add(0);
                        foreach (var ite in filteredList)
                        {
                            int temp;
                            if (int.TryParse(ite.Data[columnHeaders[i]], out temp))
                            {
                                DataToGraph[i] = DataToGraph[i] += temp;
                            }
                        }
                        DataToGraph[i] = DataToGraph[i] / filteredList.Count();
                    }
                }
                else
                {
                    int temp;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        DataToGraph.Add(0);
                        foreach (var thing in filteredList)
                        {
                            if (int.TryParse(thing.Data[columns[i]], out temp))
                            {
                                DataToGraph[i] += temp;
                            }
                        }
                        DataToGraph[i] = DataToGraph[i] / filteredList.Count();
                    }
                }
                return DataToGraph;
            }
            catch
            {
                System.Windows.MessageBox.Show("Failed to Filter groupings into data, this usually occurs when the data is in the incorrect format");
                return null;
            }
        }

        #endregion
        #region Actual Drawing of Graphs

        private void drawXandYAxis()
        {           
            Rectangle yAxis = new Rectangle
            {
                Width = 20000,
                Height = 1,
                Stroke = Brushes.Black,
            };

            Rectangle xAxis = new Rectangle
            {
                Width = 1,
                Height = GraphCanvas.ActualHeight - 199 - 20,
                Stroke = Brushes.Black,
            };

            Canvas.SetLeft(xAxis, 19);
            Canvas.SetBottom(xAxis, 199);
            Canvas.SetLeft(yAxis, 19);
            Canvas.SetBottom(yAxis, 199);
            GraphCanvas.Children.Add(xAxis);
            GraphCanvas.Children.Add(yAxis);

            List<TextBlock> labelsForYAxix = new List<TextBlock>();
            List<Rectangle> backLinesForGraph = new List<Rectangle>();
            for (int i = 0; i < 5; i++)
            {
                labelsForYAxix.Add(new TextBlock{
                    RenderTransform = new RotateTransform(270),
                    Text = $"{i * 20 + 20}%",
                });

                backLinesForGraph.Add(new Rectangle{
                    Width = 20000,
                    Height = 1,
                    Opacity = 10,
                    Fill = new SolidColorBrush(Color.FromArgb(75, 116, 125, 128)),
                });

            }
            double percentDisplacement = (GraphCanvas.ActualHeight - 200 - 20) / 5;
            for (int i = 0; i < labelsForYAxix.Count; i++)
            {
                Canvas.SetLeft(labelsForYAxix[i], 0);
                Canvas.SetBottom(labelsForYAxix[i], 200 + percentDisplacement + (i * percentDisplacement) - 30);
                GraphCanvas.Children.Add(labelsForYAxix[i]);

                Canvas.SetLeft(backLinesForGraph[i], 19);
                Canvas.SetBottom(backLinesForGraph[i], 200 + percentDisplacement + (i * percentDisplacement));
                GraphCanvas.Children.Add(backLinesForGraph[i]);
            }
        }

        private void DrawABarChart(List<int> dataToDraw, List<string> cols, List<string> labelForBars)
        {
            for (int i = 0; i < dataToDraw.Count; i++)
            {
                int heightOfBar = dataToDraw[i];
                if (dataToDraw[i] > 100)
                {
                    heightOfBar = 0;
                }

                if (cols[i].Contains("Teacher", StringComparison.InvariantCultureIgnoreCase) || cols[i].Contains("Timetable", StringComparison.InvariantCultureIgnoreCase) || cols[i].Contains("Name", StringComparison.InvariantCultureIgnoreCase) || cols[i].Contains("Ethnicity", StringComparison.InvariantCultureIgnoreCase) || cols[i].Contains("birth", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                Rectangle bar = new Rectangle
                {
                    Width = widthOfTheGraphBars,
                    Height = ((GraphCanvas.ActualHeight - 199 - 20) / 100) * heightOfBar,
                    Fill = new SolidColorBrush(ColorDictionary[cols[i]]),
                    Stroke = Brushes.Black,
                    ToolTip = cols[i].ToString() + " " + dataToDraw[i].ToString(),
                };
                Canvas.SetLeft(bar, distanceFromLeft);
                Canvas.SetBottom(bar, 200);

                if (heightOfBar != 0)
                {
                    System.Windows.Controls.Label LabelOnGraphForBars = new System.Windows.Controls.Label
                    {
                        Content = $"{labelForBars[i]}",
                        Background = new SolidColorBrush(ColorDictionary[cols[i]]),
                        Foreground = Brushes.Black,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        RenderTransform = new RotateTransform(270),
                        Height = widthOfTheGraphBars,
                        Padding =  new System.Windows.Thickness(0,0,0,0),                                         
                    };
                  
                    Canvas.SetLeft(LabelOnGraphForBars, distanceFromLeft);
                    Canvas.SetBottom(LabelOnGraphForBars, 190);
                    GraphCanvas.Children.Add(LabelOnGraphForBars);
                    
                }

                System.Windows.Controls.Label PercentageMarkLabel = new System.Windows.Controls.Label
                {
                    Content = $"{dataToDraw[i]}%",
                    Foreground = Brushes.Black,
                    RenderTransform = new RotateTransform(270),
                    Height = widthOfTheGraphBars,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Padding = new System.Windows.Thickness(0, 0, 0, 0)
                };
                GraphCanvas.Children.Add(PercentageMarkLabel);
                Canvas.SetLeft(PercentageMarkLabel, distanceFromLeft);
                Canvas.SetBottom(PercentageMarkLabel, 188 + (((GraphCanvas.ActualHeight - 199 - 20) / 100) * heightOfBar));

                System.Windows.Controls.Label textBlock = new System.Windows.Controls.Label
                {
                    Content = cols[i],
                    Height = widthOfTheGraphBars,
                    Width = 170,
                    Foreground = Brushes.Black,
                    RenderTransform = new RotateTransform(270),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                    Padding = new System.Windows.Thickness(0, 0, 5, 0),
                };
                TextBlock.SetTextAlignment(textBlock, TextAlignment.Right);
                Canvas.SetLeft(textBlock, distanceFromLeft);
                Canvas.SetBottom(textBlock, 0 + (30 - widthOfTheGraphBars));

                    GraphCanvas.Children.Add(textBlock);
                    GraphCanvas.Children.Add(bar);
                    distanceFromLeft += widthOfTheGraphBars;
                distanceFromLeft += 1; //gap between bars
                }
            distanceFromLeft += widthOfTheGraphBars;
            GraphCanvas.Width = distanceFromLeft + 20;
            GraphCanvas.UpdateLayout();         
        }
        #endregion

        /// <summary>
        /// Sets the data to be in view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewDataButton_Click(object sender, RoutedEventArgs e)
        {
            col2.Width = new GridLength(1, GridUnitType.Star);
            col3.Width = new GridLength(0, GridUnitType.Pixel);
        }

        /// <summary>
        /// sets canvas/graphs to be in view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewGraphButton_Click(object sender, RoutedEventArgs e)
        {
            col2.Width = new GridLength(0, GridUnitType.Pixel);
            col3.Width = new GridLength(1, GridUnitType.Star);
        }

        #region Initialising and Setting up Data to be Manipulated/Graphed
        /// <summary>
        /// loads the selectable variables into the listboxes for the variables to chart on the graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphProgram_Click(object sender, RoutedEventArgs e)
        {
            if (ListView1.Items.Count > 0)
            {
                foreach (string item in data[0].Split(','))
                {
                    if (!GroupResultsListbox.Items.Contains(item) &&
                        !item.Contains("Ethnicity", StringComparison.InvariantCultureIgnoreCase) && 
                        !item.Contains("Name", StringComparison.InvariantCultureIgnoreCase) &&
                        !item.Contains("Teacher", StringComparison.InvariantCultureIgnoreCase) && 
                        !item.Contains("Timetable", StringComparison.InvariantCultureIgnoreCase) && 
                        !item.Contains("Birth", StringComparison.InvariantCultureIgnoreCase)) 
                    {
                        GroupResultsListbox.Items.Add(item);
                    }
                }

                for (int i = 0; i < data[0].Split(',').Count(); i++)
                {
                    if (columnHeaders[i].Contains("name", StringComparison.InvariantCultureIgnoreCase)) continue;
                    else
                    {
                        foreach (var item in returnAllIs(data, i))
                        {                         
                            if (!GroupByListBox.Items.Contains(item) && (!double.TryParse(item, out waster) || waster > 100) 
                                && !GroupByListBox.Items.Contains(item) 
                                && !GroupResultsListbox.Items.Contains(item) 
                                && !item.Contains("Ethnicity", StringComparison.InvariantCultureIgnoreCase)
                                && !item.Contains("Timetable", StringComparison.InvariantCultureIgnoreCase)
                                && !item.Contains("Teacher", StringComparison.InvariantCultureIgnoreCase)
                                && item != "")
                            {                          
                                 GroupByListBox.Items.Add(item);
                            }
                            else continue;
                        }
                    }
                }
            }
            GroupByListBox.Items.SortDescriptions.Add( new System.ComponentModel.SortDescription
                ("", System.ComponentModel.ListSortDirection.Ascending)); //sorts the items in right textbox in ascending alphabetical order
        }

        /// <summary>
        /// finds and returns all occurrances of an item in a certain collection
        /// </summary>
        /// <param name="data"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private List<string> returnAllIs(string[] data, int number)
        {
            List<string> ret = new List<string>();
            foreach (var item in data)
            {
                ret.Add(item.Split(',')[number]);
            }
            return ret;
        }

        /// <summary>
        /// loads the data into person objects
        /// </summary>
        private void LoadPersonsObjects()
        {
            try //I actually have no clue what errors can occur here 
            {
                List<string> data1 = data.Skip(1).ToList<string>();
                string[] columnHeaders = data[0].Split(',');
                List<string> colHeaders = columnHeaders.ToList<string>();

                for (int i = 0; i < data1.Count; i++)
                {
                    Person tempPerson = new Person(columnHeaders, data1[i].Split(','));
                    if (tempPerson.Data.ContainsKey("Timetable") && tempPerson.Data.ContainsKey("Teacher"))
                    {
                        tempPerson.Data.Add("TeacherBand", $"{tempPerson.Data["Timetable"]} {tempPerson.Data["Teacher"]}");
                    }

                    listOfAllPeople.Add(tempPerson);
                }
            }
            catch { System.Windows.MessageBox.Show("Error Loading people objects"); }
        }
        #endregion

        /// <summary>
        /// resets everything by restarting the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
            Close();
        }

        #region Graphs Manipulation
        private void CopyGraphsToClipBoard_Click(object sender, RoutedEventArgs e)
        {
            if (GraphCanvas.ActualWidth > 20)
            {
                System.Windows.Forms.Clipboard.SetImage(BitmapFromWriteableBitmap(SaveAsWriteableBitmap(GraphCanvas)));
                GraphCanvas.Children.Clear();
                distanceFromLeft = 40;
                col2.Width = new GridLength(1, GridUnitType.Star);
                col3.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else System.Windows.MessageBox.Show("The canvas must be visible in order to copy to clipboard");
        }

        #region Image conversion
        /// <summary>
        /// sourced from http://stackoverflow.com/questions/16050202/the-fastest-way-to-convert-canvas-to-the-writeablebitmap-in-wpf
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        private WriteableBitmap SaveAsWriteableBitmap(Canvas surface)
        {
            if (surface == null) return null;

            Transform transform = surface.LayoutTransform;
            surface.LayoutTransform = null;

            Size size = new Size(surface.ActualWidth, surface.ActualHeight);
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
              (int)size.Width,
              (int)size.Height,
              96d,
              96d,
              PixelFormats.Pbgra32);
            renderBitmap.Render(surface);       
            surface.LayoutTransform = transform;
            return new WriteableBitmap(renderBitmap);
        }

        /// <summary>
        /// sourced from http://stackoverflow.com/questions/17298034/converting-writeablebitmap-to-bitmap-in-c-sharp
        /// </summary>
        /// <param name="writeBmp"></param>
        /// <returns></returns>
        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }
        #endregion

        private void SameImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (GraphCanvas.ActualWidth > 20)
            {
                string nameOfSave = "";
                foreach (var item in groupByCategories)
                {
                    nameOfSave += $"{item.ToString()}, ";
                }

                System.Drawing.Bitmap imageFromCanvasToSave = BitmapFromWriteableBitmap(SaveAsWriteableBitmap(GraphCanvas));

                System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
                saveDialog.DefaultExt = "jpg";
                saveDialog.Filter = "JPG images (*.jpg)|*.jpg";
                saveDialog.FileName = nameOfSave;

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IntPtr hBitmap = imageFromCanvasToSave.GetHbitmap();
                    System.Drawing.Image bmp = System.Drawing.Image.FromHbitmap(hBitmap);

                    using (var bitMapStream = new MemoryStream())
                    {
                        bmp.Save(saveDialog.FileName, ImageFormat.Jpeg);
                    }
                    bmp.Dispose();

                    col2.Width = new GridLength(1, GridUnitType.Star);
                    col3.Width = new GridLength(0, GridUnitType.Pixel);
                }
            }
            else System.Windows.MessageBox.Show("The canvas must be visible in order to save");
        }
        #endregion

        #region Likely to be Defunct Group
        /// <summary>
        /// Likely defunct method/button because hardcoded stuff usually breaks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoGenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                col2.Width = new GridLength(0, GridUnitType.Pixel);
                col3.Width = new GridLength(1, GridUnitType.Star);
                Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                UpdateLayout();
                GraphCanvas.UpdateLayout();
                //System.Threading.Thread.Sleep(1000);            

                List<string> grafsFor1Band = new List<string> { "911", "912", "913", "914" };
                List<string> grafsFor2Band = new List<string> { "921", "922", "923", "924", "925", "926", "927", "928", "929" };
                List<string> grafsFor3Band = new List<string> { "931", "932", "933" };
                List<string> colsToDay = new List<string> { "Algebra Average", "Number Average", "Prob & Stats Average", "Measurement Average", "Geometry Average", "Common Test Average" };
                //Algebra Average,Number Average,Prob & Stats Average,Measurement Average,Geometry Average,Common Test Average

                drawXandYAxis();
                foreach (var item in grafsFor1Band)
                {
                    List<int> dataToDraw = FilterGroupingsIntoDataToGraph(item, colsToDay.ToList<string>());
                    DrawAutoGenStuff(dataToDraw, colsToDay.ToList<string>(), item);
                }
                AutoSave(grafsFor1Band[0] + "Band Graphs");


                GraphCanvas.Children.Clear();
                distanceFromLeft = 40;
                drawXandYAxis();
                col3.Width = new GridLength(1200, GridUnitType.Pixel);
                GraphCanvas.UpdateLayout();
                foreach (var item in grafsFor2Band)
                {
                    List<int> dataToDraw = FilterGroupingsIntoDataToGraph(item, colsToDay.ToList<string>());
                    DrawAutoGenStuff(dataToDraw, colsToDay.ToList<string>(), item);
                }
                AutoSave(grafsFor2Band[0] + "Band Graphs");

                GraphCanvas.Children.Clear();
                distanceFromLeft = 40;
                drawXandYAxis();
                GraphCanvas.UpdateLayout();
                foreach (var item in grafsFor3Band)
                {
                    List<int> dataToDraw = FilterGroupingsIntoDataToGraph(item, colsToDay.ToList<string>());
                    DrawAutoGenStuff(dataToDraw, colsToDay.ToList<string>(), item);
                }
                AutoSave(grafsFor3Band[0] + "Band Graphs");

                col2.Width = new GridLength(1, GridUnitType.Star);
                col3.Width = new GridLength(0, GridUnitType.Pixel);
                GraphCanvas.Children.Clear();
                distanceFromLeft = 40;
            }
            catch
            {
                System.Windows.MessageBox.Show("Autogenerate only works with data in the correct format, make sure you have the bands: 911-933 and the columns: Algebra Average, Number Average, Prob & Stats Average, Measurement Average, Geometry Average, Common Test Average");
                viewDataButton_Click(sender, e);
            }
        }

        private void DrawAutoGenStuff(List<int> dataaa, List<string> colss, string item)
        {
            DrawABarChart(dataaa, colss.ToList<string>(), labelsForBars(item, colss.ToList<string>()));
        }

        private void AutoSave(string saveFileName)
        {
            System.Drawing.Bitmap imageFromCanvasToSave = BitmapFromWriteableBitmap(SaveAsWriteableBitmap(GraphCanvas));
            IntPtr hBitmap = imageFromCanvasToSave.GetHbitmap();
            System.Drawing.Image bmp = System.Drawing.Image.FromHbitmap(hBitmap);
            using (var bitMapStream = new MemoryStream())
            {
                bmp.Save(saveFileName + ".jpg", ImageFormat.Jpeg);
            }
            bmp.Dispose();
        }
        #endregion 

        private void ClearGraphs_Click(object sender, RoutedEventArgs e)
        {
            GraphCanvas.Children.Clear();
            
            col2.Width = new GridLength(1, GridUnitType.Star);
            col3.Width = new GridLength(0, GridUnitType.Pixel);
            distanceFromLeft = 40;
        }

        #region Full Band Average Graphing
        private void All1BandButton_Click(object sender, RoutedEventArgs e)
        {
            DrawBandData('1', "One Band");
        }

        private void All2BandButton_Click(object sender, RoutedEventArgs e)
        {
            DrawBandData('2', "Two Band");
        }

        private void All3BandButton_Click(object sender, RoutedEventArgs e)
        {
            DrawBandData('3', "Three Band");
        }

        private void DrawBandData(char bandToCheckFor, string labelForBar)
        {
            if (GroupByListBox.Items.Count != 0)
            {
                List<string> oneBand = new List<string>();
                foreach (var item in GroupByListBox.Items)
                {
                    string s = item.ToString();
                    if (ExcludeSelectionCheckBox.IsChecked == true)
                    {                       
                        if (((s.Count() == 3 && s[1] == bandToCheckFor) || (s.Count() == 4 && s[2] == bandToCheckFor)) && (s != "911" && s != "912" && s != "1011" && s != "1012" && s != "1013"))
                        {
                            oneBand.Add(s);
                        }
                    }
                    else
                    {
                        if ((s.Count() == 3 && s[1] == bandToCheckFor) || (s.Count() == 4 && s[2] == bandToCheckFor))
                        {
                            oneBand.Add(s);
                        }
                    }
                }

                List<string> columns = new List<string>();

                foreach (var item in GroupResultsListbox.SelectedItems)
                {
                    columns.Add(item.ToString());
                }
                if (columns.Count == 0) columns = null;

                if (WidthOfBarsComboBox.SelectedItem != null)
                {
                    ComboBoxItem typeItem = (ComboBoxItem)WidthOfBarsComboBox.SelectedItem;
                    string value = typeItem.Content.ToString();

                    int temp = 15;
                    int.TryParse(value, out temp);
                    widthOfTheGraphBars = temp;
                }

                List<int> DataToGraph = new List<int>();

                List<Person> filteredList = new List<Person>();
                foreach (var item in oneBand)
                {
                    filteredList.AddRange(listOfAllPeople.Where(p => p.Data.ContainsValue(item)));
                }

                if (columns == null)
                {
                    for (int i = 0; i < columnHeaders.Count; i++)
                    {
                        DataToGraph.Add(0);
                        foreach (var ite in filteredList)
                        {
                            int temp;
                            if (int.TryParse(ite.Data[columnHeaders[i]], out temp))
                            {
                                DataToGraph[i] = DataToGraph[i] += temp;
                            }
                        }
                        DataToGraph[i] = DataToGraph[i] / filteredList.Count();
                    }
                }
                else
                {
                    int temp;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        DataToGraph.Add(0);
                        foreach (var thing in filteredList)
                        {
                            if (int.TryParse(thing.Data[columns[i]], out temp))
                            {
                                DataToGraph[i] += temp;
                            }
                        }
                        DataToGraph[i] = DataToGraph[i] / filteredList.Count();
                    }
                }

                List<string> lebels = new List<string>();
                if (columns != null)
                {
                    foreach (var item in columns)
                    {
                        lebels.Add(labelForBar);
                    }
                }
                else
                {
                    for (int i = 0; i < columnHeaders.Count; i++)
                    {
                        lebels.Add(labelForBar);
                    }
                }
                if (columns == null)
                {
                    columns = columnHeaders.ToList<string>();
                }

                drawXandYAxis();
                DrawABarChart(DataToGraph, columns, lebels);
            }
            else System.Windows.MessageBox.Show("Please load the graph program before using this feature");
        }
        #endregion
    }
    }

    /// <summary>
    /// http://stackoverflow.com/questions/444798/case-insensitive-containsstring
    /// </summary>
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }



