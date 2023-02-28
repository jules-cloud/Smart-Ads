using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilleInteligente.Models
{
    public class Container
    {
        private string _containerName;
        private List<BlobData> _containerData;

        public Container(string name)
        {
            this.ContainerName = name;
        }
        public Container(string name, List<BlobData> data) : this(name)
        {
            this.ContainerData = data;
        }
        public string ContainerName
        {
            get { return this._containerName; }
            set { this._containerName = value; }
        }

        public List<BlobData> ContainerData
        {
            get { return this._containerData; }
            set { this._containerData = value; }
        }
    }
}
