using System;

namespace FEMur.Results
{
    /// <summary>
    /// ���v�f�̒[�����́i�f�ʗ́j���i�[����N���X
    /// </summary>
    public class ElementStress
    {
        /// <summary>
        /// �v�fID
        /// </summary>
        public int ElementId { get; set; }

        // i�[�i�n�_�j�̒f�ʗ�
        /// <summary>
        /// i�[ ���� [N]
        /// </summary>
        public double Fx_i { get; set; }

        /// <summary>
        /// i�[ ����f��Y [N]
        /// </summary>
        public double Fy_i { get; set; }

        /// <summary>
        /// i�[ ����f��Z [N]
        /// </summary>
        public double Fz_i { get; set; }

        /// <summary>
        /// i�[ �˂��胂�[�����g [N?mm]
        /// </summary>
        public double Mx_i { get; set; }

        /// <summary>
        /// i�[ �Ȃ����[�����gY [N?mm]
        /// </summary>
        public double My_i { get; set; }

        /// <summary>
        /// i�[ �Ȃ����[�����gZ [N?mm]
        /// </summary>
        public double Mz_i { get; set; }

        // j�[�i�I�_�j�̒f�ʗ�
        /// <summary>
        /// j�[ ���� [N]
        /// </summary>
        public double Fx_j { get; set; }

        /// <summary>
        /// j�[ ����f��Y [N]
        /// </summary>
        public double Fy_j { get; set; }

        /// <summary>
        /// j�[ ����f��Z [N]
        /// </summary>
        public double Fz_j { get; set; }

        /// <summary>
        /// j�[ �˂��胂�[�����g [N?mm]
        /// </summary>
        public double Mx_j { get; set; }

        /// <summary>
        /// j�[ �Ȃ����[�����gY [N?mm]
        /// </summary>
        public double My_j { get; set; }

        /// <summary>
        /// j�[ �Ȃ����[�����gZ [N?mm]
        /// </summary>
        public double Mz_j { get; set; }

        public ElementStress() { }

        public ElementStress(int elementId)
        {
            ElementId = elementId;
        }

        public override string ToString()
        {
            return $"Element {ElementId}: " +
                   $"i�[[Fx={Fx_i:F2}, Fy={Fy_i:F2}, Fz={Fz_i:F2}, Mx={Mx_i:F2}, My={My_i:F2}, Mz={Mz_i:F2}], " +
                   $"j�[[Fx={Fx_j:F2}, Fy={Fy_j:F2}, Fz={Fz_j:F2}, Mx={Mx_j:F2}, My={My_j:F2}, Mz={Mz_j:F2}]";
        }
    }
}