using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
        
        LineElement(int id,List<int> nodeIds, int materialId, int crossSectionId)
            :base(id,nodeIds,materialId,crossSectionId)
        {

        }
        LineElement(int id, List<Node> nodes, int materialId, int crossSectionId)
            : base(id, nodes.Select(n => n.Id).ToList(), materialId, crossSectionId)
        {
        }
        LineElement(int id, Node node1, Node node2, Material material, int crossSectionId)
            : base(id, new List<int> { node1.Id, node2.Id }, material., crossSectionId)
        {
        }


        public LineElement(int v1, int v2) 
        {
            NodeIds = new List<int> { v1, v2 };
        }
        public LineElement(int id, int node1Id, int node2Id, int materialId, int crossSectionId)
        {
            Id = id;
            NodeIds = new List<int> { node1Id,node2Id };
            MaterialId = materialId;
            CrossSectionId = crossSectionId;
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
