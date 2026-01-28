using System;

namespace FEMur.Common.Units
{
    /// <summary>
    /// 力の単位
    /// </summary>
    public enum ForceUnit
    {
        /// <summary>ニュートン</summary>
        N,
        /// <summary>キロニュートン</summary>
        kN
    }

    /// <summary>
    /// 長さの単位
    /// </summary>
    public enum LengthUnit
    {
        /// <summary>ミリメートル</summary>
        mm,
        /// <summary>メートル</summary>
        m
    }

    /// <summary>
    /// 単位変換と表示用のユーティリティクラス
    /// </summary>
    public static class UnitConverter
    {
        #region Force Conversion

        /// <summary>
        /// 力の単位変換係数を取得（基準単位: N）
        /// </summary>
        /// <param name="unit">変換先の単位</param>
        /// <returns>変換係数</returns>
        public static double GetForceConversionFactor(ForceUnit unit)
        {
            switch (unit)
            {
                case ForceUnit.N:
                    return 1.0; // N (基準単位)
                case ForceUnit.kN:
                    return 0.001; // N → kN
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// 力の値を指定された単位に変換
        /// </summary>
        /// <param name="valueInN">N単位の値</param>
        /// <param name="targetUnit">変換先の単位</param>
        /// <returns>変換後の値</returns>
        public static double ConvertForce(double valueInN, ForceUnit targetUnit)
        {
            return valueInN * GetForceConversionFactor(targetUnit);
        }

        #endregion

        #region Length Conversion

        /// <summary>
        /// 長さの単位変換係数を取得（基準単位: mm）
        /// </summary>
        /// <param name="unit">変換先の単位</param>
        /// <returns>変換係数</returns>
        public static double GetLengthConversionFactor(LengthUnit unit)
        {
            switch (unit)
            {
                case LengthUnit.mm:
                    return 1.0; // mm (基準単位)
                case LengthUnit.m:
                    return 0.001; // mm → m
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// 長さの値を指定された単位に変換
        /// </summary>
        /// <param name="valueInMm">mm単位の値</param>
        /// <param name="targetUnit">変換先の単位</param>
        /// <returns>変換後の値</returns>
        public static double ConvertLength(double valueInMm, LengthUnit targetUnit)
        {
            return valueInMm * GetLengthConversionFactor(targetUnit);
        }

        #endregion

        #region Moment Conversion

        /// <summary>
        /// モーメントの単位変換係数を取得（基準単位: N·mm）
        /// </summary>
        /// <param name="forceUnit">力の単位</param>
        /// <param name="lengthUnit">長さの単位</param>
        /// <returns>変換係数</returns>
        public static double GetMomentConversionFactor(ForceUnit forceUnit, LengthUnit lengthUnit)
        {
            return GetForceConversionFactor(forceUnit) * GetLengthConversionFactor(lengthUnit);
        }

        /// <summary>
        /// モーメントの値を指定された単位に変換
        /// </summary>
        /// <param name="valueInNMm">N·mm単位の値</param>
        /// <param name="forceUnit">力の単位</param>
        /// <param name="lengthUnit">長さの単位</param>
        /// <returns>変換後の値</returns>
        public static double ConvertMoment(double valueInNMm, ForceUnit forceUnit, LengthUnit lengthUnit)
        {
            return valueInNMm * GetMomentConversionFactor(forceUnit, lengthUnit);
        }

        #endregion

        #region Unit Symbol

        /// <summary>
        /// 力の単位記号を取得
        /// </summary>
        public static string GetForceUnitSymbol(ForceUnit unit)
        {
            return unit.ToString();
        }

        /// <summary>
        /// 長さの単位記号を取得
        /// </summary>
        public static string GetLengthUnitSymbol(LengthUnit unit)
        {
            return unit.ToString();
        }

        /// <summary>
        /// モーメントの単位記号を取得
        /// </summary>
        public static string GetMomentUnitSymbol(ForceUnit forceUnit, LengthUnit lengthUnit)
        {
            return $"{GetForceUnitSymbol(forceUnit)}·{GetLengthUnitSymbol(lengthUnit)}";
        }

        #endregion

        #region Formatting

        /// <summary>
        /// 力の値をフォーマットして文字列に変換
        /// </summary>
        public static string FormatForce(double valueInN, ForceUnit targetUnit, string format = "F2", bool includeUnit = true)
        {
            double converted = ConvertForce(valueInN, targetUnit);
            string valueStr = converted.ToString(format);
            return includeUnit ? $"{valueStr} {GetForceUnitSymbol(targetUnit)}" : valueStr;
        }

        /// <summary>
        /// モーメントの値をフォーマットして文字列に変換
        /// </summary>
        public static string FormatMoment(double valueInNMm, ForceUnit forceUnit, LengthUnit lengthUnit, 
            string format = "F2", bool includeUnit = true)
        {
            double converted = ConvertMoment(valueInNMm, forceUnit, lengthUnit);
            string valueStr = converted.ToString(format);
            return includeUnit ? $"{valueStr} {GetMomentUnitSymbol(forceUnit, lengthUnit)}" : valueStr;
        }

        /// <summary>
        /// 長さの値をフォーマットして文字列に変換
        /// </summary>
        public static string FormatLength(double valueInMm, LengthUnit targetUnit, string format = "F2", bool includeUnit = true)
        {
            double converted = ConvertLength(valueInMm, targetUnit);
            string valueStr = converted.ToString(format);
            return includeUnit ? $"{valueStr} {GetLengthUnitSymbol(targetUnit)}" : valueStr;
        }

        #endregion
    }
}