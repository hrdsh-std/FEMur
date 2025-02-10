using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Model
{
    public class Element
    {
        public int id { get; } = 0;
        public List<int> nodes_id { get; set; }
        public Section section { get; }
        public Material material { get; }


        public Element(int id, List<int> nodes_id, Material material, Section section)
        {
            this.id = id;
            this.nodes_id = nodes_id;
            this.section = section;
            this.material = material;
        }

        public override string ToString()
        {
            return $"Element {id}: Node {nodes_id} Material {material} Section {section}";
        }

    }
}
