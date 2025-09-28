using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.CrossSections;
using FEMur.Geometry;
using FEMur.Materials;
using FEMur.Nodes;
using Grasshopper.Kernel.Geometry.SpatialTrees;

namespace FEMur.Elements
{
    //abstract class for line elements such as beams, trusses, etc.
    public abstract class LineElement:ElementBase,ISerializable
    {
        public LineElement() { }
        
        LineElement(int id,List<int> nodeIds, Material material, CrossSection_Beam crossSection)
            :base(id,nodeIds,material,crossSection)
        {

        }
        LineElement(int id, List<Node> nodes, Material material, CrossSection_Beam crossSection)
            : base(id, nodes.Select(n => n.Id).ToList(), material, crossSection)
        {
        }
        LineElement(int id, Node node1, Node node2, Material material, CrossSection_Beam crossSection)
            : base(id, new List<int> { node1.Id, node2.Id }, material, crossSection)
        {
        }


        public LineElement(int v1, int v2) 
        {
            NodeIds = new List<int> { v1, v2 };
        }
        public LineElement(int id, int node1Id, int node2Id, Material material, CrossSection_Beam crossSection)
        {
            Id = id;
            NodeIds = new List<int> { node1Id,node2Id };
            Material = material;
            CrossSection = crossSection;
        }
        public LineElement(SerializationInfo info, StreamingContext context)
        :base(info, context)
        {
            throw new NotImplementedException();
        }
        public void disassembleElement()
        {
            throw new NotImplementedException("Disassembling line elements is not implemented yet.");
        }
    }
}
