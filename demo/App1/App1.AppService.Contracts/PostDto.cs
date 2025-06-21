using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.AppService.Contracts
{
    public class PostDto
    {
        public int? Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }
    }
}
