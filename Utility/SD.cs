using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Utility
{
    public static class SD
    {
        public const string DefaultFoodImage = "default_food.png";

        public const string ManagerUser = "Manager";
        public const string KitchenUser = "Kitchen";
        public const string FrontEndUser = "FrontDesk";
        public const string CustomerEndUser = "Customer";

        public const string ssShoppingCartCount = "ssCartCount";
        public const string ssCouponCode = "ssCouponCode";

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for(int i=0;i<source.Length;i++)
            {
                char let = source[i];
                if(let=='<')
                {
                    inside = true;
                    continue;
                }
                if(let=='>')
                {
                    inside = false;
                    continue;
                }
                if(!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
        public static double DiscountedPrice(Coupon couponFromDB,double OriginalOrderTotal)
        {
            if(couponFromDB==null)
            {
                return OriginalOrderTotal;
            }
            else
            {
                if(couponFromDB.MinimumAmount>OriginalOrderTotal)
                {
                    return OriginalOrderTotal;
                }
                else
                {
                    if (Convert.ToInt32(couponFromDB.CouponType) == (int)Coupon.ECouponType.Dollar)
                    {
                        return Math.Round(OriginalOrderTotal - couponFromDB.Discount, 2);
                    }
                    else
                    {
                        if (Convert.ToInt32(couponFromDB.CouponType) == (int)Coupon.ECouponType.Percent)
                        {
                            return Math.Round(OriginalOrderTotal - (OriginalOrderTotal * couponFromDB.Discount / 100), 2);
                        }
                    }
                }
            }
            return OriginalOrderTotal;
        }
    }
}
