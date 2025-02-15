using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Model;
using FEMur.Core.Analyze;
using Result = FEMur.Core.Analyze.Result;


namespace FEMur.Core.Model
{
    public class FEMModel
    {
        public List<Node> nodes { get; set; }
        public List<Element> elements { get;}
        public int elementType { get;}
        public List<Load> loads { get; set; }
        public List<Support> supports { get; set; }

        public Result result { get; set; }

        public FEMModel(List<Node> nodes, List<Element> elements,List<Load> loads,List<Support>supports,int elementType = 1)//elementType 1:１次要素　2:非適合要素(デフォルト) 3:２次要素
        {
            this.nodes = nodes;
            this.elements = elements;
            this.elementType = elementType;
            this.loads = loads;
            this.supports = supports;
            this.result = new Result(this);
        }
        public override string ToString()
        {
            return $"Model: Nodes {this.nodes.Count}, Elements {this.elements.Count},ElementType{this.elementType}";
        }   
    }
}
