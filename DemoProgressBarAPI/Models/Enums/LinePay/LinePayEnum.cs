namespace DemoProgressBarAPI.Models.Enums.LinePay
{ 
   public enum ConfirmUrlTypeEnum
   {
       CLIENT,
       NONE,
       SERVER
   } 
   public enum MethodEnum
    {
        /// <summary>
        /// LINE Pay餘額
        /// </summary>
        BALANCE,
        /// <summary>
        /// 信用卡（包括簽帳卡）
        /// </summary>
        CREDIT_CARD,
        /// <summary>
        /// LINE POINTS
        /// </summary>
        POINT
    }
}
