using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FindCrossingVacantions
{
    class Program
    {
        static void Main(string[] args)
        {
            string inDataFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData\\vacantions2.txt");
            string outDataFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result.txt");

            Console.WriteLine("Демо версия программы \"Поиск пересечения отпусков сотрудников предприятия\"\r\n");

            try
            {
                if (!File.Exists(inDataFileName))
                {
                    throw new FileNotFoundException($"{inDataFileName}\rФайл входных данных не найден!");
                }

                Console.WriteLine("Получение входных данных...");
                Console.WriteLine("Выполняется поиск...");
                Console.WriteLine("Найдено кол-во пересечений отпусков: {0}", FindCrossingVacantion(inDataFileName, outDataFileName));
                Console.WriteLine("\r\nРезультат запроса сохранен в файл:");
                Console.WriteLine(outDataFileName);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.Write("\r\nНажмите клавишу Enter для завершения программы...");
                Console.Read();
            }
        }

        private static int FindCrossingVacantion(string inDataFileName, string outDatafileName)
        {
            int result = 0;

            // Извлекаем коллекцию данных сотрудника
            var userVacantion = GetDataUserVacantion();

            // Извлекаем коллекцию данных с отпусками сотрудников всего предприятия
            var organizationVacantion = GetDataOrganizationVacantion(inDataFileName);

            if (organizationVacantion.Count() > 0 && userVacantion.Count() > 0)
            {
                int exceptUserID = userVacantion.FirstOrDefault().UserID;

                // Запрос всех пересечений отпусков сотрудника с отпусками сотрудников всего предприятия
                var resultQuery = organizationVacantion
                    .Select(x => new CrossVacantion
                    {
                        Vacantion = x,
                        CrossUserVacantion = userVacantion
                        .Where(y => (x.UserID != exceptUserID)
                        && ((x.StartDate <= y.StartDate && x.EndDate >= y.StartDate) || (x.StartDate <= y.EndDate && x.EndDate >= y.EndDate))
                        ).SingleOrDefault()
                    });

                // Сохраняем результат выполнения запроса
                if (resultQuery.Count() > 0)
                {
                    SaveResult(outDatafileName, resultQuery);
                }

                result = resultQuery.Where(x => x.CrossUserVacantion != null).Select(x => x).Count();
            }

            return result;
        }

        private static void SaveResult(string fileName, IEnumerable<CrossVacantion> result)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(stream);
                UserVacantion vacantion;
                UserVacantion crossUserVacantion;

                foreach (var item in result)
                {
                    vacantion = item.Vacantion;
                    crossUserVacantion = item.CrossUserVacantion;

                    if (crossUserVacantion != null)
                    {
                        writer.WriteLine($"ID\t{vacantion.UserID}\t{vacantion.StartDate.ToString("dd.MM.yyyy")}-{vacantion.EndDate.ToString("dd.MM.yyyy")} пересекается с {crossUserVacantion.StartDate.ToString("dd.MM.yyyy")}-{crossUserVacantion.EndDate.ToString("dd.MM.yyyy")}");
                    }
                    else
                    {
                        writer.WriteLine($"ID\t{vacantion.UserID}\t{vacantion.StartDate.ToString("dd.MM.yyyy")}-{vacantion.EndDate.ToString("dd.MM.yyyy")}");
                    }
                }

                writer.Close();
            }
        }

        private static IList<UserVacantion> GetDataOrganizationVacantion(string fileName)
        {
            IList<UserVacantion> rows = new List<UserVacantion>();

            using (StreamReader reader = new StreamReader(fileName))
            {
                UserVacantion row;
                string line = reader.ReadLine();
                string[] values = null;

                while (line != null)
                {
                    values = line.Split('\t', '-');

                    if (values.Length == 4)
                    {
                        row = new UserVacantion
                        {
                            UserID = Convert.ToInt32(values[1]),
                            StartDate = Convert.ToDateTime(values[2]),
                            EndDate = Convert.ToDateTime(values[3])
                        };

                        rows.Add(row);
                    }

                    line = reader.ReadLine();
                }
            }

            return rows;
        }

        private static IList<UserVacantion> GetDataUserVacantion()
        {
            IList<UserVacantion> rows = new List<UserVacantion>()
            {
                new UserVacantion {
                    UserID = 1,
                    StartDate = new DateTime(2023, 2, 20),
                    EndDate = new DateTime(2023, 2, 27)
                },

                new UserVacantion {
                    UserID = 1,
                    StartDate = new DateTime(2023, 9, 15),
                    EndDate = new DateTime(2023, 9, 28)
                }
            };

            return rows;
        }
    }
}
