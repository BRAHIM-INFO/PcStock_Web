namespace PcStock_Web.Models
{
    // تعليق: هذا الكلاس يمثل بنية المقال في قاعدة بيانات كوزيدار
    public class Article
    {
        public string REF { get; set; }        // المرجع
        public string CODE_INT { get; set; }   // الكود الداخلي
        public string INTITULE { get; set; }   // التسمية الرئيسية
        public string INTITULE2 { get; set; }  // التسمية الثانوية
        public string FAMILLE { get; set; }    // العائلة
        public double QTE { get; set; }        // الكمية
        public double PAMP { get; set; }       // متوسط السعر المرجح
        public double MONTANT => QTE * PAMP;   // المبلغ (محسوب تلقائياً)
        public string DATE_MAJ { get; set; }   // تاريخ التحديث
    }
}