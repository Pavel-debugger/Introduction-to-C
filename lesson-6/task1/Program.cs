﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                if (args[0] == "/l") ListAllProcesses(args);
                else if (args[0] == "/k") KillProcess(args);
            }
            else Help();
        }

        private static void KillProcess(string[] arg)
        {
            if (arg.Length > 1)
            {
                string[] process = arg[1].Split('.');

                try
                {
                    Process forKill = Process.GetProcessById(Convert.ToInt32(process[0]));
                    forKill.Kill();
                    Console.WriteLine($"Процесс с идентификатором {process[0]} завершен.");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Процесс с идентификатором {process[0]} не выполняется.");
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    Console.WriteLine("Отказано в доступе");
                }
                catch (FormatException)
                {
                    try
                    {
                        Process[] forKill = Process.GetProcessesByName(process[0]);
                        forKill[0].Kill();
                        Console.WriteLine($"Процесс {arg[1]} завершен.");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine($"Процесс {arg[1]} не выполняется.");
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        Console.WriteLine("Отказано в доступе");
                    }

                }

            } else Help();

        }

        private static void ListAllProcesses(string[] arg)
        {
            Process[] all = Process.GetProcesses();

            if (arg.Length > 1 && arg[1] == "i")
            {
                Sort(all, true);
            }
            else
            {
                Sort(all,false);
            }

            Console.WriteLine("ID\tИмя");
            for (int i =0; i < all.Length;i++)
            {
                Console.WriteLine($"{all[i].Id}\t{all[i].ProcessName}");
            }
        }

        private static void Sort(Process[] all, bool sort)
        {
            for (int j = 0; j < all.Length - 1; j++)
            {
                for (int i = 0; i < all.Length - 1; i++)
                {
                    if (!sort ? all[i].ProcessName.CompareTo(all[i + 1].ProcessName) > 0 : all[i].Id.CompareTo(all[i + 1].Id) > 0)
                    {
                        Process tmp = all[i + 1];
                        all[i + 1] = all[i];
                        all[i] = tmp;
                    }
                }
            }
        }

        private static void Help()
        {
            Console.WriteLine("task.exe [параметр]");
            Console.WriteLine("Параметры:");
            Console.WriteLine(" /l [i]\tСписок запушеных процессов.");
            Console.WriteLine("\ti - сортировка по ID.");
            Console.WriteLine(" /k [Имя или ID процесса]\n\tЗавершить процесс.");
        }
    }
}
