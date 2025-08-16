using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.CrossSections;
using FEMur.Geometry;
using FEMur.Materials;

namespace FEMur.Elements
{
    public class ShellElement : ElementBase,ISerializable
    {
        public int Id { get; }
        public Mesh3 Mesh { get; set; }
        public CrossSection CrossSection { get;}
        public Material Material { get; }
        public ShellElement(int id, CrossSection crossSection, Material material)
        {
            Id = id;
            CrossSection = crossSection;
            Material = material;
        }


    }
}
