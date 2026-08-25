using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.GenerateCode
{
    public class Generate
    {
        public static string GenerateC()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                int c = 0;

                while (c < 6)
                {
                    stringBuilder.Append(Random.Shared.Next(1, 10));
                    c++;
                }
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ошибка генерации кода" + ex.Message);
                return string.Empty;
            }
        }
    }
}
