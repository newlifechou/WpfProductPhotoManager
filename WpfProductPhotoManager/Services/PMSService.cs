using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProductPhotoManager.PMS;

namespace WpfProductPhotoManager.Services
{
    public class PMSService
    {
        public List<string> GetProductIds()
        {
            Random random = new Random();
            var ids = new List<string>();
            for (int i = 0; i < 500; i++)
            {
                ids.Add($"{DateTime.Today.AddDays(random.Next(-365, 0)).ToString("yyMMdd")}-{(char)('A' + random.Next(0, 7))}-{random.Next(1, 4)}");
            }
            return ids;
        }

        public List<string> GetProductIdsFromPMS()
        {
            List<string> productids = new List<string>();
            using (var client = new RecordTestServiceClient())
            {
                var results = client.GetAllProductID();
                productids = results.Select(i => i[0]).ToList();
            }
            return productids;
        }

    }
}
