using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilleInteligente.Models
{
    public class BlobData
    {
        private byte[] _dataContent;
        private string _dataName;

        public BlobData(byte[] content, string name)
        {
            DataContent = content;
            DataName = name;
        }

        public byte[] DataContent
        {
            get { return _dataContent; }
            set { _dataContent = value; }
        }

        public string DataName
        {
            get { return _dataName; }
            set { _dataName = value; }
        }
    }
}
