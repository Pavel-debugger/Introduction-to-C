﻿using System;
using System.IO;
using System.Linq;

namespace FileManager
{
    class Program
    {
        static int numberItems = 10;
        static int pos = 0;
        static bool drive = false;
        static void Main()
        {



            Console.Clear();
            int width = Console.WindowWidth < 120 ? 120 : Console.WindowWidth;
            int height = numberItems + 6;
            Console.SetWindowSize(width + 1, height);
            Console.SetBufferSize(width + 1, height);

            //рисуем рамку
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if ((i == 0 && j == 0) || (i == 0 && j == height - 1) ||
                        (i == width - 1 && j == 0) || (i == width - 1 && j == height - 1)) PrintSym(i, j, '+');
                    if (i > 0 && i < width - 1 && (j == 0 || j == height - 1)) PrintSym(i, j, '=');
                    if (j > 0 && j < height - 1 && (i == 0 || i == width - 1)) PrintSym(i, j, '|');
                    if (i > 0 && i < width - 1 && (j == 2 || j == height - 3)) PrintSym(i, j, '-');
                }

            string[] listFiles = ChageDir();
            string[] list = PrintList(listFiles);

            int posTmp = 0;
            int shift = 0;
            int curPos = 1;
            string cmd = "";
            //навигация
            while (true)
            {
                if (shift >= listFiles.Length - list.Length) shift = listFiles.Length - list.Length;
                if (shift < 0) shift = 0;
                if (posTmp != pos)
                {
                    if (pos < 0)
                    {
                        if (list.First() != "..")
                            list = PrintList(listFiles, shift--);
                        pos = 0;
                    }
                    else if (pos >= list.Length)
                    {
                        if (list.Last() != listFiles.Last().Split('\\').Last())
                            list = PrintList(listFiles, ++shift);
                        pos = list.Length - 1;
                    }


                    PrintLine(1, 3 + posTmp, list[posTmp], list[posTmp].Length, true);
                    PrintLine(1, 3 + pos, list[pos], list[pos].Length, true, true);
                }

                posTmp = pos;

                Console.SetCursorPosition(curPos, height - 2);
                Console.CursorVisible = true;
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.F10) return;
                else if (key.Key == ConsoleKey.DownArrow) pos++;
                else if (key.Key == ConsoleKey.UpArrow) pos--;
                else if (key.Key == ConsoleKey.PageDown)
                {
                    if (pos != 0)
                    {
                        shift += pos;
                        pos = list.Length;
                    }
                    else pos = list.Length - 1;
                }
                else if (key.Key == ConsoleKey.PageUp)
                {
                    if (pos != list.Length - 1)
                    {
                        shift -= list.Length - pos;
                        pos = -1;
                    }
                    else pos = 0;
                }
                else if (key.Key == ConsoleKey.F2)
                {
                    long sizeDir = SumSizeDir(list[pos]);
                    PrintLine(1, pos + 3, list[pos], list[pos].Length, true, true, Convert.ToString(sizeDir));
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (cmd != "")
                    {
                        PrintLine(1, height - 2, " ", cmd.Length);
                        curPos = 1;
                        CommandHandler(cmd);
                        cmd = "";
                        listFiles = ChageDir();
                        list = PrintList(listFiles);
                    }
                    else if (Directory.GetParent(Directory.GetCurrentDirectory()) == null && list[pos] == "..")
                    {
                        drive = true;
                        listFiles = Directory.GetLogicalDrives();
                        list = PrintList(listFiles);
                        PrintLine(1, 1, " ", Console.BufferWidth - 3);
                    }
                    else
                    {
                        listFiles = ChageDir(list[pos]);
                        list = PrintList(listFiles);
                        posTmp = pos;
                        shift = 0;
                    }
                }
                // реализация ввода команд с клавиатуры
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (cmd.Length > 0)
                    {
                        Console.Write(" ");
                        cmd = cmd.Remove(cmd.Length - 1);
                        curPos--;
                    }
                }
                else
                {
                    cmd += key.KeyChar;
                    curPos++;
                }
                Console.CursorVisible = false;
            }




        }
        //обработка комманд
        private static void CommandHandler(string cmd)
        {
            string[] cmdList = new string[3];
            string cmdTmp = "";
            int count = 0;
            for (int i = 0; i < cmd.Length; i++)
            {
                if (cmd[i] != ' ') cmdTmp += cmd[i];
                else
                {
                    cmdList[count] = cmdTmp;
                    cmdTmp = "";
                    count++;
                }
            }
            cmdList[count] = cmdTmp;

            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                if (cmdList[0] == "copy" || cmdList[0] == "COPY")
                {
                    char op1Last = cmdList[1][cmdList[1].Length - 1];
                    char op2Last = cmdList[2][cmdList[2].Length - 1];
                    string path1 = cmdList[1];
                    string path2 = cmdList[2];

                    if (op1Last == '\\' && op2Last == '\\')
                    {
                        if (Directory.Exists(path1)) CopyDir(path1, path2);
                        else
                        {
                            path1 = currentDir + '\\' + cmdList[1];
                            if (Directory.Exists(path1)) CopyDir(path1, path2);
                        }
                    }
                    else if (op1Last != '\\' && op2Last != '\\')
                    {
                        if (File.Exists(path1)) File.Copy(path1, path2, true);
                        else
                        {
                            path1 = currentDir + '\\' + cmdList[1];
                            if (File.Exists(path1)) File.Copy(path1, path2, true);
                        }
                    }
                    else if (op1Last != '\\' && op2Last == '\\')
                    {
                        if (File.Exists(path1)) File.Copy(path1, path2 + path1, true);
                        else
                        {
                            path1 = currentDir + '\\' + cmdList[1];
                            if (File.Exists(path1)) File.Copy(path1, path2 + path1, true);
                        }
                    }
                }
                else if (cmdList[0] == "del" || cmdList[0] == "DEL")
                {
                    char op1Last = cmdList[1][cmdList[1].Length - 1];
                    string path = cmdList[1];
                    if (op1Last == '\\')
                    {
                        if (Directory.Exists(cmdList[1])) DelDir(path);
                        else
                        {
                            path = currentDir + '\\' + cmdList[1];
                            if (Directory.Exists(path)) DelDir(path);
                        }
                    }
                    else
                    {
                        if (File.Exists(cmdList[1])) File.Delete(path);
                        else
                        {
                            path = currentDir + '\\' + cmdList[1];
                            if (File.Exists(path)) File.Delete(path);
                        }
                    }
                }
            }
            catch
            {

            }
        }
        //копирование директории
        static void CopyDir(string source, string destination)
        {
            string[] listForCopy = Directory.GetDirectories(source, "*", SearchOption.AllDirectories);
            for (int i = 0; i < listForCopy.Length; i++)
                if (!Directory.Exists(destination + listForCopy[i]))
                    Directory.CreateDirectory(destination + listForCopy[i]);
            listForCopy = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
            for (int i = 0; i < listForCopy.Length; i++)
            {
                File.Copy(listForCopy[i], destination + listForCopy[i], true);
            }
        }
        //удаление директории
        static void DelDir(string path)
        {
            string[] del = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            for (int i = 0; i < del.Length; i++)
                File.Delete(del[i]);
            del = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            for (int i = del.Length - 1; i >= 0; i--)
                Directory.Delete(del[i]);
            Directory.Delete(path);
        }



        static void ClearArea()
        {
            for (int i = 3; i < Console.BufferHeight - 3; i++)
                for (int j = 1; j < Console.BufferWidth - 2; j++)
                    PrintSym(j, i, ' ');
        }
        static string[] ChageDir(string dir = "")
        {
            DirectoryInfo currentDir;
            if (dir == "..") currentDir = new DirectoryInfo(Directory.GetParent(Directory.GetCurrentDirectory()).FullName);
            else
            {
                if (!drive)
                {
                    currentDir = new DirectoryInfo(Directory.GetCurrentDirectory() + '\\' + dir);

                }
                else
                {
                    currentDir = new DirectoryInfo(dir);
                    drive = false;
                }

            }
            try
            {
                Directory.SetCurrentDirectory(currentDir.FullName);
                pos = 0;
            }
            catch//System.IO.IOException,System.UnauthorizedAccessException
            {
                Directory.SetCurrentDirectory(Directory.GetParent(currentDir.FullName).FullName);
                currentDir = Directory.GetParent(currentDir.FullName);
            }

            PrintLine(1, 1, currentDir.FullName, Console.BufferWidth - 3);
            try
            {
                string[] listFiles = Directory.GetFileSystemEntries(currentDir.FullName);

                return listFiles;
            }
            catch //System.UnauthorizedAccessException,System.IO.IOException
            {
                string[] listFiles = Directory.GetFileSystemEntries(Directory.GetParent(currentDir.FullName).FullName);
                Directory.SetCurrentDirectory(Directory.GetParent(currentDir.FullName).FullName);
                return listFiles;
            }


        }
        static string[] PrintList(string[] listFiles, int shift = 0)
        {
            ClearArea();
            int count = listFiles.Length - shift;
            string[] listForPrint = new string[(numberItems - 1 > count ? count : numberItems - 1) + 1];
            if (!drive)
            {
                for (int i = 0; i < listForPrint.Length; i++)
                {
                    if (i == 0 && shift == 0) listForPrint[0] = "..";
                    else
                        listForPrint[i] = listFiles[i - 1 + shift].Split('\\').Last();
                    bool selectLine;
                    if (i == pos) selectLine = true;
                    else selectLine = false;
                    PrintLine(1, 3 + i, listForPrint[i], listForPrint[i].Length, true, selectLine);
                }

            }
            else
            {
                for (int i = 0; i < listFiles.Length; i++)
                {
                    bool selectLine;
                    if (i == pos) selectLine = true;
                    else selectLine = false;
                    PrintLine(1, 3 + i, listFiles[i], listFiles[i].Length, true, selectLine);
                }
                listForPrint = listFiles;
            }

            return listForPrint;
        }
        static void PrintSym(int x, int y, char sym)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(sym);
        }
        static void PrintLine(int x, int y, string line, int length, bool attr = false, bool select = false, string sizeDir = "")
        {
            Console.SetCursorPosition(x, y);
            if (select)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            if (line.Length != 0 && attr && line != "..")
            {
                line = AddAttributeFiles(line, sizeDir);
                string[] lineSplit = line.Split('\t');
                line = FormattingLine(lineSplit[0], 35) + '\t' + FormattingLine(lineSplit[1], 4) + '\t' + FormattingLine(lineSplit[2], 4) + '\t' +
                    FormattingLine(lineSplit[3], 20) + '\t' + FormattingLine(lineSplit[4], 35);
            }
            for (int i = 0; i < (length < line.Length ? line.Length : length); i++)
            {
                if (i < line.Length) Console.Write(line[i]);
                else Console.Write(" ");
            }
            Console.ResetColor();
        }

        private static string FormattingLine(string line, int lenght)
        {
            string lineForPrint = "";
            for (int j = 0; j < lenght; j++)
            {
                if (j < line.Length) lineForPrint += line[j];
                else lineForPrint += " ";
            }
            return lineForPrint;
        }

        //вычисление размера директории
        static long SumSizeDir(string list)
        {
            long sum = 0;
            try
            {
                string[] dirList = Directory.GetDirectories(list);
                for (int i = 0; i < dirList.Length; i++) sum += SumSizeDir(dirList[i]);

                DirectoryInfo dir = new DirectoryInfo(list);
                FileInfo[] filesLenght = dir.GetFiles();
                for (int j = 0; j < filesLenght.Length; j++) sum += filesLenght[j].Length;
            }
            catch { }
            return sum;
        }

        //добавление атрибутов файла
        static string AddAttributeFiles(string file, string sizeDir = "")
        {
            //FileAttributes fileAttr = File.GetAttributes(file);
            string attr = "";
            FileInfo fileAttr = new FileInfo(file);
            string dir = "FILE";
            if ((fileAttr.Attributes & FileAttributes.Archive) == FileAttributes.Archive) attr += "A";
            if ((fileAttr.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) attr += "R";
            if ((fileAttr.Attributes & FileAttributes.System) == FileAttributes.System) attr += "S";
            if ((fileAttr.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) attr += "H";
            if ((fileAttr.Attributes & FileAttributes.Directory) == FileAttributes.Directory) dir = "DIR";
            DateTime time = File.GetLastWriteTime(file);
            file += "\t" + dir + "\t" + attr + "\t" + time + "\t" + 
                ((fileAttr.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? ((sizeDir!="")? sizeDir + " байт" : ""): fileAttr.Length + " байт");
            return file;
        }

    }
}
