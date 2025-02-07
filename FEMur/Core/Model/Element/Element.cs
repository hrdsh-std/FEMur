using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Rhino.DocObjects;
using FEMur.Core.Model;

namespace FEMur.Core.Model.Element
{
    internal class Element
    {
        int id { get; } = 0;
        List<Node.Node> nodes { get; set; }
        Section.Section section { get; }
        Material.Material material{  get; }

        
        public Element(int id , List<Node.Node> nodes, Material.Material material, Section.Section section)
        {
            this.id = id;
            this.nodes = nodes;
            this.section = section;
            this.material = material;
        }

        public override string ToString()
        {
            return $"Element {this.id}: Node {this.nodes} Material {this.material} Section {this.section}";
        }

    }
}
