using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfProductPhotoManager.Services
{
    public class PMSService
    {
        public List<string> GetProductIds()
        {
            Random random = new Random();
            var ids = new List<string>();
            for (int i = 0; i < 30; i++)
            {
                ids.Add($"{DateTime.Today.AddDays(random.Next(-30, 0)).ToString("yyMMdd")}-{(char)('A' + random.Next(0, 7))}-{random.Next(1, 4)}");
            }
            return ids;
        }
    }
}
