using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Praktika_foxes_and_chikens
{
    public partial class Form1 : Form
    {
        //Объявление глобальных переменных
        const int map_size = 7;   //размер игрового поля
        const int cell_size = 50;   //размер одной клетки игрового поля

        char[,] map = new char[map_size, map_size];   //макет игрового поля из символов
        Button[,] cells = new Button[map_size, map_size];    //макет игрового поля из кнопок

        //вид клеток игрового поля
        //(картинки заранее были отрисованы в фигме разсером 50*50)
        Image fox_icon;
        Image chicken_icon;
        Image bush_icon;
        Image trail_icon;

        //задний фон клетки
        Image select_bcgr;
        Image fox_bcgr;
        Image chicken_bcgr;
        Image bush_bcgr;
        Image trail_bcgr;
        Image step_bcgr;

        char current_player;   //определение текущего игрока

        Button previous_cell;   //предыдущая клетка
        Button select_cell;   //выбранная клетка
        bool active_cell;   //определение текущей клетки

        int count_eat_step = 0;   //кол-во съедобных ходов (для лис)
        bool continue_eat_step = false;   //определение возможно ли съесть еще кур, после первого хода

        List<Button> simple_steps = new List<Button>();   //запись обычных ходов

        public Form1()
        {
            InitializeComponent();

            fox_icon = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\icons\icon_fox.png");
            chicken_icon = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\icons\icon_chicken.png");
            bush_icon = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\icons\icon_wall.png");
            trail_icon = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\icons\icon_null.png");

            select_bcgr = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\bcgr\select_bcgr.png");
            fox_bcgr = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\bcgr\fox_bcgr.png");
            chicken_bcgr = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\bcgr\chicken_bcgr.png");
            bush_bcgr = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\bcgr\bush_bcgr.png");
            trail_bcgr = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\bcgr\trail_bcgr.png");
            step_bcgr = new Bitmap(@"C:\Users\Александра\Desktop\Praktika_foxes_and_chikens\bcgr\step_bcgr.png");

            Init_game();
        }

        //Обработка кнопки для перезапуска игры
        private void btn_reset_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();
            Init_game();

        }

        //Функция запуска игры
        public void Init_game()
        {
            current_player = 'к';
            active_cell = false;
            previous_cell = null;

            map = new char[7, 7]   //макет игрового поля (# - кусты, ' ' - тропинка, л - лисы, к - куры)
            {
                { '#', '#', ' ', ' ', ' ', '#', '#' },
                { '#', '#', ' ', ' ', ' ', '#', '#' },
                { ' ', ' ', 'л', ' ', 'л', ' ', ' ' },
                { 'к', 'к', 'к', 'к', 'к', 'к', 'к' },
                { 'к', 'к', 'к', 'к', 'к', 'к', 'к' },
                { '#', '#', 'к', 'к', 'к', '#', '#' },
                { '#', '#', 'к', 'к', 'к', '#', '#' },
            };

            Create_map();
        }

        //Функция создания игрового поля
        public void Create_map()
        {
            //размер рабочего окна задаем через свойства формы (366; 389)
            //выводим массив из кнопок размером 50*50

            for (int i = 0; i < map_size; i++)
            {
                for (int j = 0; j < map_size; j++)
                {
                    Button cell = new Button();
                    cell.Size = new Size(cell_size, cell_size);
                    cell.Location = new Point(j * cell_size, i * cell_size);

                    if (map[i, j] == '#') cell.Image = bush_icon;
                    if (map[i, j] == ' ') cell.Image = trail_icon;
                    if (map[i, j] == 'л') cell.Image = fox_icon;
                    if (map[i, j] == 'к') cell.Image = chicken_icon;

                    cell.BackgroundImage = Get_cell_BackgroundImage(cell);

                    cell.Click += new EventHandler(Selection_cell);

                    this.Controls.Add(cell);
                    cells[i, j] = cell;
                }
            }
        }

        //Функция смены игрока (ходы выполняются по очереди)
        public void Choice_player()
        {
            current_player = current_player == 'к' ? 'л' : 'к';
        }

        //Функция смены заднего фона клетки игрового поля
        public Image Get_cell_BackgroundImage(Button previous_cell)
        {
            if (map[previous_cell.Location.Y / cell_size, previous_cell.Location.X / cell_size] == '#') return bush_bcgr;
            else if (map[previous_cell.Location.Y / cell_size, previous_cell.Location.X / cell_size] == 'л') return fox_bcgr;
            else if (map[previous_cell.Location.Y / cell_size, previous_cell.Location.X / cell_size] == 'к') return chicken_bcgr;
            else return trail_bcgr;
        }

        //Функция выбора клетки для хода + выполнение хода
        public void Selection_cell(object sender, EventArgs e)
        {
            if (previous_cell != null) previous_cell.BackgroundImage = Get_cell_BackgroundImage(previous_cell);

            Button select_cell = sender as Button;

            //выбираем клетку для хода
            if (map[select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size] != '#' &&
                map[select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size] != ' ' &&
                map[select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size] == current_player)
            {
                Delete_steps();
                select_cell.BackgroundImage = select_bcgr;
                Deactivate_cell();

                select_cell.Enabled = true;
                Show_steps_player(select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size, map[select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size]);

                if (active_cell)
                {
                    Activate_cell();
                    Delete_steps();
                    select_cell.BackgroundImage = Get_cell_BackgroundImage(select_cell);
                    active_cell = false;
                }
                else active_cell = true;
            }
            //делаем ход
            else
            {
                //меняем предыдущую клетку и активную местами
                if (active_cell)
                {
                    Activate_cell();
                    Delete_steps();

                    char select_char = map[select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size];   //символ выбраной клетки на карте
                    map[select_cell.Location.Y / cell_size, select_cell.Location.X / cell_size] = map[previous_cell.Location.Y / cell_size, previous_cell.Location.X / cell_size];
                    map[previous_cell.Location.Y / cell_size, previous_cell.Location.X / cell_size] = select_char;

                    select_cell.Image = previous_cell.Image;
                    select_cell.BackgroundImage = previous_cell.BackgroundImage;
                    previous_cell.Image = trail_icon;
                    previous_cell.BackgroundImage = trail_bcgr;
                    active_cell = false;

                    Choice_player();   //смена игроков
                }
            }

            previous_cell = select_cell;
        }

        //Функция для отключения кнопок
        //(для показа возможных ходов при выборе лисы/курицы)
        //(для показа потенциального хода лисы (поедание курицы))
        public void Deactivate_cell()
        {
            for (int i = 0; i < map_size; i++)
            {
                for (int j = 0; j < map_size; j++)
                {
                    cells[i, j].Enabled = false;
                }
            }
        }

        //Функция для включения кнопок
        public void Activate_cell()
        {
            for (int i = 0; i < map_size; i++)
            {
                for (int j = 0; j < map_size; j++)
                {
                    cells[i, j].Enabled = true;
                }
            }
        }

        //Фукция для проверки находится ли индекс шага в рамках игрального поля
        public bool Step_inside_map(int new_i, int new_j)
        {
            if (new_i >= map_size || new_i < 0 || new_j >= map_size || new_j < 0) return false;
            else return true;
        }

        //Функция для очистки истории шагов лисы/курицы
        //(каждой кнопке присваиваем вид, который она имела)
        public void Delete_steps()
        {
            for (int i = 0; i < map_size; i++)
            {
                for (int j = 0; j < map_size; j++)
                {
                    cells[i, j].BackgroundImage = Get_cell_BackgroundImage(cells[i, j]);
                }
            }
        }

        //Функция показа возможных ходов лисы/курицы
        public void Show_steps_player(int new_i, int new_j, char current_char)
        {
            int select_step = current_player == 'к' ? -1 : 1;

            switch (current_char)
            {
                case 'к':
                    //обычный ход вниз
                    if (Step_inside_map(new_i + 1 * select_step, new_j))
                    {
                        if (map[new_i + 1 * select_step, new_j] == ' ')
                        {
                            cells[new_i + 1 * select_step, new_j].BackgroundImage = step_bcgr;
                            cells[new_i + 1 * select_step, new_j].Enabled = true;
                        }
                    }

                    //обычный ход вправо
                    if (Step_inside_map(new_i, new_j + 1 * select_step))
                    {
                        if (map[new_i, new_j + 1 * select_step] == ' ')
                        {
                            cells[new_i, new_j + 1 * select_step].BackgroundImage = step_bcgr;
                            cells[new_i, new_j + 1 * select_step].Enabled = true;
                        }
                    }

                    //обычный ход влево
                    if (Step_inside_map(new_i, new_j - 1 * select_step))
                    {
                        if (map[new_i, new_j - 1 * select_step] == ' ')
                        {
                            cells[new_i, new_j - 1 * select_step].BackgroundImage = step_bcgr;
                            cells[new_i, new_j - 1 * select_step].Enabled = true;
                        }
                    }

                    break;

                case 'л':
                    //обычный ход вниз
                    if (Step_inside_map(new_i + 1 * select_step, new_j))
                    {
                        if (map[new_i + 1 * select_step, new_j] == ' ')
                        {
                            cells[new_i + 1 * select_step, new_j].BackgroundImage = step_bcgr;
                            cells[new_i + 1 * select_step, new_j].Enabled = true;
                        }
                    }

                    //обычный ход вверх
                    if (Step_inside_map(new_i - 1 * select_step, new_j))
                    {
                        if (map[new_i - 1 * select_step, new_j] == ' ')
                        {
                            cells[new_i - 1 * select_step, new_j].BackgroundImage = step_bcgr;
                            cells[new_i - 1 * select_step, new_j].Enabled = true;
                        }
                    }

                    //обычный ход вправо
                    if (Step_inside_map(new_i, new_j + 1 * select_step))
                    {
                        if (map[new_i, new_j + 1 * select_step] == ' ')
                        {
                            cells[new_i, new_j + 1 * select_step].BackgroundImage = step_bcgr;
                            cells[new_i, new_j + 1 * select_step].Enabled = true;
                        }
                    }

                    //обычный ход влево
                    if (Step_inside_map(new_i, new_j - 1 * select_step))
                    {
                        if (map[new_i, new_j - 1 * select_step] == ' ')
                        {
                            cells[new_i, new_j - 1 * select_step].BackgroundImage = step_bcgr;
                            cells[new_i, new_j - 1 * select_step].Enabled = true;
                        }
                    }

                    /*------------------------------------------------------------------------*/

                    //съедобный ход вниз
                    if (Step_inside_map(new_i + 2 * select_step, new_j))
                    {
                        if (map[new_i + 2 * select_step, new_j] == ' ' && map[new_i + 1 * select_step, new_j] == 'к')
                        {
                            cells[new_i + 2 * select_step, new_j].BackgroundImage = step_bcgr;
                            cells[new_i + 2 * select_step, new_j].Enabled = true;
                        }
                    }

                    //съедобный ход вверх
                    if (Step_inside_map(new_i - 2 * select_step, new_j))
                    {
                        if (map[new_i - 2 * select_step, new_j] == ' ' && map[new_i - 1 * select_step, new_j] == 'к')
                        {
                            cells[new_i - 2 * select_step, new_j].BackgroundImage = step_bcgr;
                            cells[new_i - 2 * select_step, new_j].Enabled = true;
                        }
                    }

                    //съедобный ход вправо
                    if (Step_inside_map(new_i, new_j + 2 * select_step))
                    {
                        if (map[new_i, new_j + 2 * select_step] == ' ' && map[new_i, new_j + 1 * select_step] == 'к')
                        {
                            cells[new_i, new_j + 2 * select_step].BackgroundImage = step_bcgr;
                            cells[new_i, new_j + 2 * select_step].Enabled = true;
                        }
                    }

                    //съедобный ход влево
                    if (Step_inside_map(new_i, new_j - 2 * select_step))
                    {
                        if (map[new_i, new_j - 2 * select_step] == ' ' && map[new_i, new_j - 1 * select_step] == 'к')
                        {
                            cells[new_i, new_j - 2 * select_step].BackgroundImage = step_bcgr;
                            cells[new_i, new_j - 2 * select_step].Enabled = true;
                        }
                    }

                    break;

            }
        }
    }
}
