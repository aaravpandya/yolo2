using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace marcu5yolo.Models
{
    public class MessageModel
    {
        public int MessageModelId { get; set; }
        public string uri1 { get; set; }
        public string uri2 { get; set; }
        public string intendedClient { get; set; }
        public string filename1 { get; set; }
        public string filename2 { get; set; }
        public string guid { get; set; }
    }
}
