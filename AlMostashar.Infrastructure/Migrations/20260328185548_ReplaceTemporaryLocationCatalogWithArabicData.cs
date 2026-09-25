using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTemporaryLocationCatalogWithArabicData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_Cities_CityId",
                table: "ClientRequests");

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 601);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 701);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 801);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 901);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1101);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1201);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1301);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1401);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1501);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1601);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1701);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1801);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1901);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2101);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2201);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2301);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2401);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2501);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2601);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2701);

            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Governorates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Cities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Al Mafrouza", 3, "المفروزة" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "El Montaza", 3, "المنتزه" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Mansheya", 3, "المنشية" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Naseria", 3, "الناصرية" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Ambrozo", 3, "امبروزو" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Fayed", 9, "فايد" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Qantara Sharq", 9, "القنطرة شرق" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Qantara Gharb", 9, "القنطرة غرب" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "El Tal El Kabier", 9, "التل الكبير" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Abu Sawir", 9, "أبو صوير" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "alruwda", 19, "الروضة" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Kafr El-Batikh", 19, "كفر البطيخ" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Azbet Al Burg", 19, "عزبة البرج" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "EnglishName", "GovernorateId", "Name" },
                values: new object[] { "Meet Abou Ghalib", 19, "ميت أبو غالب" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "EnglishName", "GovernorateId", "Name" },
                values: new object[,]
                {
                    { 1, "15 May", 1, "15 مايو" },
                    { 2, "Al Azbakeyah", 1, "الازبكية" },
                    { 3, "Al Basatin", 1, "البساتين" },
                    { 4, "Tebin", 1, "التبين" },
                    { 5, "El-Khalifa", 1, "الخليفة" },
                    { 6, "El darrasa", 1, "الدراسة" },
                    { 7, "Aldarb Alahmar", 1, "الدرب الاحمر" },
                    { 8, "Zawya al-Hamra", 1, "الزاوية الحمراء" },
                    { 9, "El-Zaytoun", 1, "الزيتون" },
                    { 10, "Sahel", 1, "الساحل" },
                    { 11, "El Salam", 1, "السلام" },
                    { 12, "Sayeda Zeinab", 1, "السيدة زينب" },
                    { 13, "El Sharabeya", 1, "الشرابية" },
                    { 14, "Shorouk", 1, "مدينة الشروق" },
                    { 15, "El Daher", 1, "الظاهر" },
                    { 16, "Ataba", 1, "العتبة" },
                    { 17, "New Cairo", 1, "القاهرة الجديدة" },
                    { 18, "El Marg", 1, "المرج" },
                    { 19, "Ezbet el Nakhl", 1, "عزبة النخل" },
                    { 20, "Matareya", 1, "المطرية" },
                    { 21, "Maadi", 1, "المعادى" },
                    { 22, "Maasara", 1, "المعصرة" },
                    { 23, "Mokattam", 1, "المقطم" },
                    { 24, "Manyal", 1, "المنيل" },
                    { 25, "Mosky", 1, "الموسكى" },
                    { 26, "Nozha", 1, "النزهة" },
                    { 27, "Waily", 1, "الوايلى" },
                    { 28, "Bab al-Shereia", 1, "باب الشعرية" },
                    { 29, "Bolaq", 1, "بولاق" },
                    { 30, "Garden City", 1, "جاردن سيتى" },
                    { 31, "Hadayek El-Kobba", 1, "حدائق القبة" },
                    { 32, "Helwan", 1, "حلوان" },
                    { 33, "Dar Al Salam", 1, "دار السلام" },
                    { 34, "Shubra", 1, "شبرا" },
                    { 35, "Tura", 1, "طره" },
                    { 36, "Abdeen", 1, "عابدين" },
                    { 37, "Abaseya", 1, "عباسية" },
                    { 38, "Ain Shams", 1, "عين شمس" },
                    { 39, "Nasr City", 1, "مدينة نصر" },
                    { 40, "New Heliopolis", 1, "مصر الجديدة" },
                    { 41, "Masr Al Qadima", 1, "مصر القديمة" },
                    { 42, "Mansheya Nasir", 1, "منشية ناصر" },
                    { 43, "Badr City", 1, "مدينة بدر" },
                    { 44, "Obour City", 1, "مدينة العبور" },
                    { 45, "Cairo Downtown", 1, "وسط البلد" },
                    { 46, "Zamalek", 1, "الزمالك" },
                    { 47, "Kasr El Nile", 1, "قصر النيل" },
                    { 48, "Rehab", 1, "الرحاب" },
                    { 49, "Katameya", 1, "القطامية" },
                    { 50, "Madinty", 1, "مدينتي" },
                    { 51, "Rod Alfarag", 1, "روض الفرج" },
                    { 52, "Sheraton", 1, "شيراتون" },
                    { 53, "El-Gamaleya", 1, "الجمالية" },
                    { 54, "10th of Ramadan City", 1, "العاشر من رمضان" },
                    { 55, "Helmeyat Alzaytoun", 1, "الحلمية" },
                    { 56, "New Nozha", 1, "النزهة الجديدة" },
                    { 57, "Capital New", 1, "العاصمة الإدارية" },
                    { 58, "Giza", 2, "الجيزة" },
                    { 59, "Sixth of October", 2, "السادس من أكتوبر" },
                    { 60, "Cheikh Zayed", 2, "الشيخ زايد" },
                    { 61, "Hawamdiyah", 2, "الحوامدية" },
                    { 62, "Al Badrasheen", 2, "البدرشين" },
                    { 63, "Saf", 2, "الصف" },
                    { 64, "Atfih", 2, "أطفيح" },
                    { 65, "Al Ayat", 2, "العياط" },
                    { 66, "Al-Bawaiti", 2, "الباويطي" },
                    { 67, "ManshiyetAl Qanater", 2, "منشأة القناطر" },
                    { 68, "Oaseem", 2, "أوسيم" },
                    { 69, "Kerdasa", 2, "كرداسة" },
                    { 70, "Abu Nomros", 2, "أبو النمرس" },
                    { 71, "Kafr Ghati", 2, "كفر غطاطي" },
                    { 72, "Manshiyet Al Bakari", 2, "منشأة البكاري" },
                    { 73, "Dokki", 2, "الدقى" },
                    { 74, "Agouza", 2, "العجوزة" },
                    { 75, "Haram", 2, "الهرم" },
                    { 76, "Warraq", 2, "الوراق" },
                    { 77, "Imbaba", 2, "امبابة" },
                    { 78, "Boulaq Dakrour", 2, "بولاق الدكرور" },
                    { 79, "Al Wahat Al Baharia", 2, "الواحات البحرية" },
                    { 80, "Omraneya", 2, "العمرانية" },
                    { 81, "Moneeb", 2, "المنيب" },
                    { 82, "Bin Alsarayat", 2, "بين السرايات" },
                    { 83, "Kit Kat", 2, "الكيت كات" },
                    { 84, "Mohandessin", 2, "المهندسين" },
                    { 85, "Faisal", 2, "فيصل" },
                    { 86, "Abu Rawash", 2, "أبو رواش" },
                    { 87, "Hadayek Alahram", 2, "حدائق الأهرام" },
                    { 88, "Haraneya", 2, "الحرانية" },
                    { 89, "Hadayek October", 2, "حدائق اكتوبر" },
                    { 90, "Saft Allaban", 2, "صفط اللبن" },
                    { 91, "Smart Village", 2, "القرية الذكية" },
                    { 92, "Ard Ellwaa", 2, "ارض اللواء" },
                    { 93, "Abu Qir", 3, "ابو قير" },
                    { 94, "Al Ibrahimeyah", 3, "الابراهيمية" },
                    { 95, "Azarita", 3, "الأزاريطة" },
                    { 96, "Anfoushi", 3, "الانفوشى" },
                    { 97, "Dekheila", 3, "الدخيلة" },
                    { 98, "El Soyof", 3, "السيوف" },
                    { 99, "Ameria", 3, "العامرية" },
                    { 100, "El Labban", 3, "اللبان" },
                    { 106, "Bab Sharq", 3, "باب شرق" },
                    { 107, "Bourj Alarab", 3, "برج العرب" },
                    { 108, "Stanley", 3, "ستانلى" },
                    { 109, "Smouha", 3, "سموحة" },
                    { 110, "Sidi Bishr", 3, "سيدى بشر" },
                    { 111, "Shads", 3, "شدس" },
                    { 112, "Gheet Alenab", 3, "غيط العنب" },
                    { 113, "Fleming", 3, "فلمينج" },
                    { 114, "Victoria", 3, "فيكتوريا" },
                    { 115, "Camp Shizar", 3, "كامب شيزار" },
                    { 116, "Karmooz", 3, "كرموز" },
                    { 117, "Mahta Alraml", 3, "محطة الرمل" },
                    { 118, "Mina El-Basal", 3, "مينا البصل" },
                    { 119, "Asafra", 3, "العصافرة" },
                    { 120, "Agamy", 3, "العجمي" },
                    { 121, "Bakos", 3, "بكوس" },
                    { 122, "Boulkly", 3, "بولكلي" },
                    { 123, "Cleopatra", 3, "كليوباترا" },
                    { 124, "Glim", 3, "جليم" },
                    { 125, "Al Mamurah", 3, "المعمورة" },
                    { 126, "Al Mandara", 3, "المندرة" },
                    { 127, "Moharam Bek", 3, "محرم بك" },
                    { 128, "Elshatby", 3, "الشاطبي" },
                    { 129, "Sidi Gaber", 3, "سيدي جابر" },
                    { 130, "North Coast/sahel", 3, "الساحل الشمالي" },
                    { 131, "Alhadra", 3, "الحضرة" },
                    { 132, "Alattarin", 3, "العطارين" },
                    { 133, "Sidi Kerir", 3, "سيدي كرير" },
                    { 134, "Elgomrok", 3, "الجمرك" },
                    { 135, "Al Max", 3, "المكس" },
                    { 136, "Marina", 3, "مارينا" },
                    { 137, "Mansoura", 4, "المنصورة" },
                    { 138, "Talkha", 4, "طلخا" },
                    { 139, "Mitt Ghamr", 4, "ميت غمر" },
                    { 140, "Dekernes", 4, "دكرنس" },
                    { 141, "Aga", 4, "أجا" },
                    { 142, "Menia El Nasr", 4, "منية النصر" },
                    { 143, "Sinbillawin", 4, "السنبلاوين" },
                    { 144, "El Kurdi", 4, "الكردي" },
                    { 145, "Bani Ubaid", 4, "بني عبيد" },
                    { 146, "Al Manzala", 4, "المنزلة" },
                    { 147, "tami al'amdid", 4, "تمي الأمديد" },
                    { 148, "aljamalia", 4, "الجمالية" },
                    { 149, "Sherbin", 4, "شربين" },
                    { 150, "Mataria", 4, "المطرية" },
                    { 151, "Belqas", 4, "بلقاس" },
                    { 152, "Meet Salsil", 4, "ميت سلسيل" },
                    { 153, "Gamasa", 4, "جمصة" },
                    { 154, "Mahalat Damana", 4, "محلة دمنة" },
                    { 155, "Nabroh", 4, "نبروه" },
                    { 156, "Hurghada", 5, "الغردقة" },
                    { 157, "Ras Ghareb", 5, "رأس غارب" },
                    { 158, "Safaga", 5, "سفاجا" },
                    { 159, "El Qusiar", 5, "القصير" },
                    { 160, "Marsa Alam", 5, "مرسى علم" },
                    { 161, "Shalatin", 5, "الشلاتين" },
                    { 162, "Halaib", 5, "حلايب" },
                    { 163, "Aldahar", 5, "الدهار" },
                    { 164, "Damanhour", 6, "دمنهور" },
                    { 165, "Kafr El Dawar", 6, "كفر الدوار" },
                    { 166, "Rashid", 6, "رشيد" },
                    { 167, "Edco", 6, "إدكو" },
                    { 168, "Abu al-Matamir", 6, "أبو المطامير" },
                    { 169, "Abu Homs", 6, "أبو حمص" },
                    { 170, "Delengat", 6, "الدلنجات" },
                    { 171, "Mahmoudiyah", 6, "المحمودية" },
                    { 172, "Rahmaniyah", 6, "الرحمانية" },
                    { 173, "Itai Baroud", 6, "إيتاي البارود" },
                    { 174, "Housh Eissa", 6, "حوش عيسى" },
                    { 175, "Shubrakhit", 6, "شبراخيت" },
                    { 176, "Kom Hamada", 6, "كوم حمادة" },
                    { 177, "Badr", 6, "بدر" },
                    { 178, "Wadi Natrun", 6, "وادي النطرون" },
                    { 179, "New Nubaria", 6, "النوبارية الجديدة" },
                    { 180, "Alnoubareya", 6, "النوبارية" },
                    { 181, "Fayoum", 7, "الفيوم" },
                    { 182, "Fayoum El Gedida", 7, "الفيوم الجديدة" },
                    { 183, "Tamiya", 7, "طامية" },
                    { 184, "Snores", 7, "سنورس" },
                    { 185, "Etsa", 7, "إطسا" },
                    { 186, "Epschway", 7, "إبشواي" },
                    { 187, "Yusuf El Sediaq", 7, "يوسف الصديق" },
                    { 188, "Hadqa", 7, "الحادقة" },
                    { 189, "Atsa", 7, "اطسا" },
                    { 190, "Algamaa", 7, "الجامعة" },
                    { 191, "Sayala", 7, "السيالة" },
                    { 192, "Tanta", 8, "طنطا" },
                    { 193, "Al Mahalla Al Kobra", 8, "المحلة الكبرى" },
                    { 194, "Kafr El Zayat", 8, "كفر الزيات" },
                    { 195, "Zefta", 8, "زفتى" },
                    { 196, "El Santa", 8, "السنطة" },
                    { 197, "Qutour", 8, "قطور" },
                    { 198, "Basion", 8, "بسيون" },
                    { 199, "Samannoud", 8, "سمنود" },
                    { 200, "Ismailia", 9, "الإسماعيلية" },
                    { 206, "Kasasien El Gedida", 9, "القصاصين الجديدة" },
                    { 207, "Nefesha", 9, "نفيشة" },
                    { 208, "Sheikh Zayed", 9, "الشيخ زايد" },
                    { 209, "Shbeen El Koom", 10, "شبين الكوم" },
                    { 210, "Sadat City", 10, "مدينة السادات" },
                    { 211, "Menouf", 10, "منوف" },
                    { 212, "Sars El-Layan", 10, "سرس الليان" },
                    { 213, "Ashmon", 10, "أشمون" },
                    { 214, "Al Bagor", 10, "الباجور" },
                    { 215, "Quesna", 10, "قويسنا" },
                    { 216, "Berkat El Saba", 10, "بركة السبع" },
                    { 217, "Tala", 10, "تلا" },
                    { 218, "Al Shohada", 10, "الشهداء" },
                    { 219, "Minya", 11, "المنيا" },
                    { 220, "Minya El Gedida", 11, "المنيا الجديدة" },
                    { 221, "El Adwa", 11, "العدوة" },
                    { 222, "Magagha", 11, "مغاغة" },
                    { 223, "Bani Mazar", 11, "بني مزار" },
                    { 224, "Mattay", 11, "مطاي" },
                    { 225, "Samalut", 11, "سمالوط" },
                    { 226, "Madinat El Fekria", 11, "المدينة الفكرية" },
                    { 227, "Meloy", 11, "ملوي" },
                    { 228, "Deir Mawas", 11, "دير مواس" },
                    { 229, "Abu Qurqas", 11, "ابو قرقاص" },
                    { 230, "Ard Sultan", 11, "ارض سلطان" },
                    { 231, "Banha", 12, "بنها" },
                    { 232, "Qalyub", 12, "قليوب" },
                    { 233, "Shubra Al Khaimah", 12, "شبرا الخيمة" },
                    { 234, "Al Qanater Charity", 12, "القناطر الخيرية" },
                    { 235, "Khanka", 12, "الخانكة" },
                    { 236, "Kafr Shukr", 12, "كفر شكر" },
                    { 237, "Tukh", 12, "طوخ" },
                    { 238, "Qaha", 12, "قها" },
                    { 239, "Obour", 12, "العبور" },
                    { 240, "Khosous", 12, "الخصوص" },
                    { 241, "Shibin Al Qanater", 12, "شبين القناطر" },
                    { 242, "Mostorod", 12, "مسطرد" },
                    { 243, "El Kharga", 13, "الخارجة" },
                    { 244, "Paris", 13, "باريس" },
                    { 245, "Mout", 13, "موط" },
                    { 246, "Farafra", 13, "الفرافرة" },
                    { 247, "Balat", 13, "بلاط" },
                    { 248, "Dakhla", 13, "الداخلة" },
                    { 249, "Suez", 14, "السويس" },
                    { 250, "Alganayen", 14, "الجناين" },
                    { 251, "Ataqah", 14, "عتاقة" },
                    { 252, "Ain Sokhna", 14, "العين السخنة" },
                    { 253, "Faysal", 14, "فيصل" },
                    { 254, "Aswan", 15, "أسوان" },
                    { 255, "Aswan El Gedida", 15, "أسوان الجديدة" },
                    { 256, "Drau", 15, "دراو" },
                    { 257, "Kom Ombo", 15, "كوم أمبو" },
                    { 258, "Nasr Al Nuba", 15, "نصر النوبة" },
                    { 259, "Kalabsha", 15, "كلابشة" },
                    { 260, "Edfu", 15, "إدفو" },
                    { 261, "Al-Radisiyah", 15, "الرديسية" },
                    { 262, "Al Basilia", 15, "البصيلية" },
                    { 263, "Al Sibaeia", 15, "السباعية" },
                    { 264, "Abo Simbl Al Siyahia", 15, "ابوسمبل السياحية" },
                    { 265, "Marsa Alam", 15, "مرسى علم" },
                    { 266, "Assiut", 16, "أسيوط" },
                    { 267, "Assiut El Gedida", 16, "أسيوط الجديدة" },
                    { 268, "Dayrout", 16, "ديروط" },
                    { 269, "Manfalut", 16, "منفلوط" },
                    { 270, "Qusiya", 16, "القوصية" },
                    { 271, "Abnoub", 16, "أبنوب" },
                    { 272, "Abu Tig", 16, "أبو تيج" },
                    { 273, "El Ghanaim", 16, "الغنايم" },
                    { 274, "Sahel Selim", 16, "ساحل سليم" },
                    { 275, "El Badari", 16, "البداري" },
                    { 276, "Sidfa", 16, "صدفا" },
                    { 277, "Bani Sweif", 17, "بني سويف" },
                    { 278, "Beni Suef El Gedida", 17, "بني سويف الجديدة" },
                    { 279, "Al Wasta", 17, "الواسطى" },
                    { 280, "Naser", 17, "ناصر" },
                    { 281, "Ehnasia", 17, "إهناسيا" },
                    { 282, "beba", 17, "ببا" },
                    { 283, "Fashn", 17, "الفشن" },
                    { 284, "Somasta", 17, "سمسطا" },
                    { 285, "Alabbaseri", 17, "الاباصيرى" },
                    { 286, "Mokbel", 17, "مقبل" },
                    { 287, "PorSaid", 18, "بورسعيد" },
                    { 288, "Port Fouad", 18, "بورفؤاد" },
                    { 289, "Alarab", 18, "العرب" },
                    { 290, "Zohour", 18, "حى الزهور" },
                    { 291, "Alsharq", 18, "حى الشرق" },
                    { 292, "Aldawahi", 18, "حى الضواحى" },
                    { 293, "Almanakh", 18, "حى المناخ" },
                    { 294, "Mubarak", 18, "حى مبارك" },
                    { 295, "Damietta", 19, "دمياط" },
                    { 296, "New Damietta", 19, "دمياط الجديدة" },
                    { 297, "Ras El Bar", 19, "رأس البر" },
                    { 298, "Faraskour", 19, "فارسكور" },
                    { 299, "Zarqa", 19, "الزرقا" },
                    { 300, "alsaru", 19, "السرو" },
                    { 305, "Kafr Saad", 19, "كفر سعد" },
                    { 306, "Zagazig", 20, "الزقازيق" },
                    { 307, "Al Ashr Men Ramadan", 20, "العاشر من رمضان" },
                    { 308, "Minya Al Qamh", 20, "منيا القمح" },
                    { 309, "Belbeis", 20, "بلبيس" },
                    { 310, "Mashtoul El Souq", 20, "مشتول السوق" },
                    { 311, "Qenaiat", 20, "القنايات" },
                    { 312, "Abu Hammad", 20, "أبو حماد" },
                    { 313, "El Qurain", 20, "القرين" },
                    { 314, "Hehia", 20, "ههيا" },
                    { 315, "Abu Kabir", 20, "أبو كبير" },
                    { 316, "Faccus", 20, "فاقوس" },
                    { 317, "El Salihia El Gedida", 20, "الصالحية الجديدة" },
                    { 318, "Al Ibrahimiyah", 20, "الإبراهيمية" },
                    { 319, "Deirb Negm", 20, "ديرب نجم" },
                    { 320, "Kafr Saqr", 20, "كفر صقر" },
                    { 321, "Awlad Saqr", 20, "أولاد صقر" },
                    { 322, "Husseiniya", 20, "الحسينية" },
                    { 323, "san alhajar alqablia", 20, "صان الحجر القبلية" },
                    { 324, "Manshayat Abu Omar", 20, "منشأة أبو عمر" },
                    { 325, "Al Toor", 21, "الطور" },
                    { 326, "Sharm El-Shaikh", 21, "شرم الشيخ" },
                    { 327, "Dahab", 21, "دهب" },
                    { 328, "Nuweiba", 21, "نويبع" },
                    { 329, "Taba", 21, "طابا" },
                    { 330, "Saint Catherine", 21, "سانت كاترين" },
                    { 331, "Abu Redis", 21, "أبو رديس" },
                    { 332, "Abu Zenaima", 21, "أبو زنيمة" },
                    { 333, "Ras Sidr", 21, "رأس سدر" },
                    { 334, "Kafr El Sheikh", 22, "كفر الشيخ" },
                    { 335, "Kafr El Sheikh Downtown", 22, "وسط البلد كفر الشيخ" },
                    { 336, "Desouq", 22, "دسوق" },
                    { 337, "Fooh", 22, "فوه" },
                    { 338, "Metobas", 22, "مطوبس" },
                    { 339, "Burg Al Burullus", 22, "برج البرلس" },
                    { 340, "Baltim", 22, "بلطيم" },
                    { 341, "Masief Baltim", 22, "مصيف بلطيم" },
                    { 342, "Hamol", 22, "الحامول" },
                    { 343, "Bella", 22, "بيلا" },
                    { 344, "Riyadh", 22, "الرياض" },
                    { 345, "Sidi Salm", 22, "سيدي سالم" },
                    { 346, "Qellen", 22, "قلين" },
                    { 347, "Sidi Ghazi", 22, "سيدي غازي" },
                    { 348, "Marsa Matrouh", 23, "مرسى مطروح" },
                    { 349, "El Hamam", 23, "الحمام" },
                    { 350, "Alamein", 23, "العلمين" },
                    { 351, "Dabaa", 23, "الضبعة" },
                    { 352, "Al-Nagila", 23, "النجيلة" },
                    { 353, "Sidi Brani", 23, "سيدي براني" },
                    { 354, "Salloum", 23, "السلوم" },
                    { 355, "Siwa", 23, "سيوة" },
                    { 356, "Marina", 23, "مارينا" },
                    { 357, "North Coast", 23, "الساحل الشمالى" },
                    { 358, "Luxor", 24, "الأقصر" },
                    { 359, "New Luxor", 24, "الأقصر الجديدة" },
                    { 360, "Esna", 24, "إسنا" },
                    { 361, "New Tiba", 24, "طيبة الجديدة" },
                    { 362, "Al ziynia", 24, "الزينية" },
                    { 363, "Al Bayadieh", 24, "البياضية" },
                    { 364, "Al Qarna", 24, "القرنة" },
                    { 365, "Armant", 24, "أرمنت" },
                    { 366, "Al Tud", 24, "الطود" },
                    { 367, "Qena", 25, "قنا" },
                    { 368, "New Qena", 25, "قنا الجديدة" },
                    { 369, "Abu Tesht", 25, "ابو طشت" },
                    { 370, "Nag Hammadi", 25, "نجع حمادي" },
                    { 371, "Deshna", 25, "دشنا" },
                    { 372, "Alwaqf", 25, "الوقف" },
                    { 373, "Qaft", 25, "قفط" },
                    { 374, "Naqada", 25, "نقادة" },
                    { 375, "Farshout", 25, "فرشوط" },
                    { 376, "Quos", 25, "قوص" },
                    { 377, "Arish", 26, "العريش" },
                    { 378, "Sheikh Zowaid", 26, "الشيخ زويد" },
                    { 379, "Nakhl", 26, "نخل" },
                    { 380, "Rafah", 26, "رفح" },
                    { 381, "Bir al-Abed", 26, "بئر العبد" },
                    { 382, "Al Hasana", 26, "الحسنة" },
                    { 383, "Sohag", 27, "سوهاج" },
                    { 384, "Sohag El Gedida", 27, "سوهاج الجديدة" },
                    { 385, "Akhmeem", 27, "أخميم" },
                    { 386, "Akhmim El Gedida", 27, "أخميم الجديدة" },
                    { 387, "Albalina", 27, "البلينا" },
                    { 388, "El Maragha", 27, "المراغة" },
                    { 389, "almunsha'a", 27, "المنشأة" },
                    { 390, "Dar AISalaam", 27, "دار السلام" },
                    { 391, "Gerga", 27, "جرجا" },
                    { 392, "Jahina Al Gharbia", 27, "جهينة الغربية" },
                    { 393, "Saqilatuh", 27, "ساقلته" },
                    { 394, "Tama", 27, "طما" },
                    { 395, "Tahta", 27, "طهطا" },
                    { 396, "Alkawthar", 27, "الكوثر" }
                });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Cairo", "القاهرة" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Giza", "الجيزة" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Alexandria", "الأسكندرية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Dakahlia", "الدقهلية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Red Sea", "البحر الأحمر" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Beheira", "البحيرة" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Fayoum", "الفيوم" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Gharbiya", "الغربية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Ismailia", "الإسماعلية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Menofia", "المنوفية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Minya", "المنيا" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Qaliubiya", "القليوبية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "New Valley", "الوادي الجديد" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Suez", "السويس" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Aswan", "اسوان" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Assiut", "اسيوط" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Beni Suef", "بني سويف" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Port Said", "بورسعيد" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Damietta", "دمياط" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Sharkia", "الشرقية" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "South Sinai", "جنوب سيناء" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Kafr Al sheikh", "كفر الشيخ" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Matrouh", "مطروح" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Luxor", "الأقصر" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Qena", "قنا" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "North Sinai", "شمال سيناء" });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "EnglishName", "Name" },
                values: new object[] { "Sohag", "سوهاج" });

            migrationBuilder.Sql(
                """
                UPDATE cr
                SET
                    cr.GovernorateId = COALESCE(cr.GovernorateId, g.Id),
                    cr.Governorate = g.Name
                FROM ClientRequests AS cr
                INNER JOIN Governorates AS g
                    ON g.Id = cr.GovernorateId
                    OR (
                        LTRIM(RTRIM(ISNULL(cr.Governorate, N''))) <> N''
                        AND (
                            UPPER(LTRIM(RTRIM(cr.Governorate))) = UPPER(g.Name)
                            OR UPPER(LTRIM(RTRIM(cr.Governorate))) = UPPER(g.EnglishName)
                        )
                    );

                ;WITH LegacyRequestCities AS
                (
                    SELECT
                        cr.Id,
                        cr.GovernorateId,
                        LegacyCityName = LTRIM(RTRIM(ISNULL(cr.City, N''))),
                        CanonicalEnglishName =
                            CASE UPPER(LTRIM(RTRIM(ISNULL(cr.City, N''))))
                                WHEN 'HELIOPOLIS' THEN 'New Heliopolis'
                                WHEN '6TH OF OCTOBER' THEN 'Sixth of October'
                                WHEN 'SHEIKH ZAYED' THEN 'Cheikh Zayed'
                                WHEN 'DAMANHUR' THEN 'Damanhour'
                                WHEN 'SHIBIN EL KOM' THEN 'Shbeen El Koom'
                                WHEN 'BENHA' THEN 'Banha'
                                WHEN 'KHARGA' THEN 'El Kharga'
                                WHEN 'BORG EL ARAB' THEN 'Bourj Alarab'
                                WHEN 'BENI SUEF' THEN 'Bani Sweif'
                                WHEN 'PORT SAID' THEN 'PorSaid'
                                WHEN 'SHARM EL SHEIKH' THEN 'Sharm El-Shaikh'
                                ELSE LTRIM(RTRIM(ISNULL(cr.City, N'')))
                            END
                    FROM ClientRequests AS cr
                    WHERE LTRIM(RTRIM(ISNULL(cr.City, N''))) <> N''
                ),
                ResolvedCities AS
                (
                    SELECT
                        lrc.Id AS RequestId,
                        c.Id AS CityId,
                        c.Name AS CityName
                    FROM LegacyRequestCities AS lrc
                    INNER JOIN Cities AS c
                        ON c.GovernorateId = lrc.GovernorateId
                        AND (
                            UPPER(LTRIM(RTRIM(c.Name))) = UPPER(lrc.LegacyCityName)
                            OR UPPER(LTRIM(RTRIM(c.EnglishName))) = UPPER(lrc.CanonicalEnglishName)
                        )
                )
                UPDATE cr
                SET
                    cr.CityId = rc.CityId,
                    cr.City = rc.CityName
                FROM ClientRequests AS cr
                INNER JOIN ResolvedCities AS rc
                    ON rc.RequestId = cr.Id;

                UPDATE cr
                SET cr.CityId = NULL
                FROM ClientRequests AS cr
                WHERE cr.CityId IS NOT NULL
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM Cities AS c
                      WHERE c.Id = cr.CityId
                        AND (
                            UPPER(LTRIM(RTRIM(ISNULL(cr.City, N'')))) = UPPER(c.Name)
                            OR UPPER(LTRIM(RTRIM(ISNULL(cr.City, N'')))) = UPPER(c.EnglishName)
                        )
                  );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_Cities_CityId",
                table: "ClientRequests",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_Cities_CityId",
                table: "ClientRequests");

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 149);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 150);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 151);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 152);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 153);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 154);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 155);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 156);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 157);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 158);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 159);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 160);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 161);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 162);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 163);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 164);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 165);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 166);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 167);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 168);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 169);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 170);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 171);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 172);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 173);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 174);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 175);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 176);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 177);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 178);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 179);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 180);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 181);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 182);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 183);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 184);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 185);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 186);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 187);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 188);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 189);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 190);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 191);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 192);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 193);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 194);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 195);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 196);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 197);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 198);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 199);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 206);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 209);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 214);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 215);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 216);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 217);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 218);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 219);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 220);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 221);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 222);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 223);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 224);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 225);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 226);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 227);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 228);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 229);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 230);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 231);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 232);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 233);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 234);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 235);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 236);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 237);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 238);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 239);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 240);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 241);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 242);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 243);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 244);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 245);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 246);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 247);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 248);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 249);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 250);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 251);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 252);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 253);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 254);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 255);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 256);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 257);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 258);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 259);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 260);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 261);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 262);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 263);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 264);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 265);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 266);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 267);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 268);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 269);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 270);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 271);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 272);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 273);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 274);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 275);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 276);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 277);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 278);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 279);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 280);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 281);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 282);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 283);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 284);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 285);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 286);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 287);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 288);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 289);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 290);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 291);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 292);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 293);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 294);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 295);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 296);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 297);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 298);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 299);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 305);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 306);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 307);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 308);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 309);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 310);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 311);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 312);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 313);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 314);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 315);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 316);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 317);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 318);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 319);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 320);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 321);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 322);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 323);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 324);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 325);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 326);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 327);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 328);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 329);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 330);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 331);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 332);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 333);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 334);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 335);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 336);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 337);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 338);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 339);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 340);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 341);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 342);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 343);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 344);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 345);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 346);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 347);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 348);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 349);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 350);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 351);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 352);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 353);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 354);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 355);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 356);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 357);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 358);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 359);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 360);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 361);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 362);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 363);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 364);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 365);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 366);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 367);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 368);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 369);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 370);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 371);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 372);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 373);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 374);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 375);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 376);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 377);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 378);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 379);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 380);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 381);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 382);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 383);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 384);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 385);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 386);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 387);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 388);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 389);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 390);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 391);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 392);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 393);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 394);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 395);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 396);

            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Cities");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 1, "Cairo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 1, "Nasr City" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 1, "Heliopolis" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 1, "Maadi" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 1, "New Cairo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 2, "Giza" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 2, "Dokki" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 2, "Mohandessin" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 2, "6th of October" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 2, "Sheikh Zayed" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 3, "Alexandria" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 3, "Smouha" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 3, "Miami" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "GovernorateId", "Name" },
                values: new object[] { 3, "Borg El Arab" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "GovernorateId", "Name" },
                values: new object[,]
                {
                    { 401, 4, "Mansoura" },
                    { 501, 5, "Hurghada" },
                    { 601, 6, "Damanhur" },
                    { 701, 7, "Fayoum" },
                    { 801, 8, "Tanta" },
                    { 901, 9, "Ismailia" },
                    { 1001, 10, "Shibin El Kom" },
                    { 1101, 11, "Minya" },
                    { 1201, 12, "Benha" },
                    { 1301, 13, "Kharga" },
                    { 1401, 14, "Suez" },
                    { 1501, 15, "Aswan" },
                    { 1601, 16, "Assiut" },
                    { 1701, 17, "Beni Suef" },
                    { 1801, 18, "Port Said" },
                    { 1901, 19, "Damietta" },
                    { 2001, 20, "Zagazig" },
                    { 2101, 21, "Sharm El Sheikh" },
                    { 2201, 22, "Kafr El Sheikh" },
                    { 2301, 23, "Marsa Matrouh" },
                    { 2401, 24, "Luxor" },
                    { 2501, 25, "Qena" },
                    { 2601, 26, "Arish" },
                    { 2701, 27, "Sohag" }
                });

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Cairo");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Giza");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Alexandria");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Dakahlia");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Red Sea");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Beheira");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Fayoum");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Gharbia");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Ismailia");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Menofia");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Minya");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Qalyubia");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "New Valley");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Suez");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Aswan");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Assiut");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "Name",
                value: "Beni Suef");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "Name",
                value: "Port Said");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 19,
                column: "Name",
                value: "Damietta");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 20,
                column: "Name",
                value: "Sharkia");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 21,
                column: "Name",
                value: "South Sinai");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 22,
                column: "Name",
                value: "Kafr El Sheikh");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 23,
                column: "Name",
                value: "Matrouh");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 24,
                column: "Name",
                value: "Luxor");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 25,
                column: "Name",
                value: "Qena");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 26,
                column: "Name",
                value: "North Sinai");

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 27,
                column: "Name",
                value: "Sohag");

            migrationBuilder.Sql(
                """
                UPDATE cr
                SET
                    cr.Governorate = g.Name,
                    cr.GovernorateId = COALESCE(cr.GovernorateId, g.Id)
                FROM ClientRequests AS cr
                INNER JOIN Governorates AS g
                    ON g.Id = cr.GovernorateId;

                ;WITH LegacyRequestCities AS
                (
                    SELECT
                        cr.Id,
                        cr.GovernorateId,
                        LegacyCityName = LTRIM(RTRIM(ISNULL(cr.City, N''))),
                        LegacyArabicCityName = LTRIM(RTRIM(ISNULL(cr.City, N''))),
                        LegacyEnglishName =
                            CASE UPPER(LTRIM(RTRIM(ISNULL(cr.City, N''))))
                                WHEN N'مصر الجديدة' THEN 'Heliopolis'
                                WHEN N'السادس من أكتوبر' THEN '6th of October'
                                WHEN N'الشيخ زايد' THEN 'Sheikh Zayed'
                                WHEN N'دمنهور' THEN 'Damanhur'
                                WHEN N'شبين الكوم' THEN 'Shibin El Kom'
                                WHEN N'بنها' THEN 'Benha'
                                WHEN N'الخارجة' THEN 'Kharga'
                                WHEN N'برج العرب' THEN 'Borg El Arab'
                                WHEN N'بني سويف' THEN 'Beni Suef'
                                WHEN N'بورسعيد' THEN 'Port Said'
                                WHEN N'شرم الشيخ' THEN 'Sharm El Sheikh'
                                ELSE LTRIM(RTRIM(ISNULL(cr.City, N'')))
                            END
                    FROM ClientRequests AS cr
                    WHERE LTRIM(RTRIM(ISNULL(cr.City, N''))) <> N''
                ),
                ResolvedCities AS
                (
                    SELECT
                        lrc.Id AS RequestId,
                        v.CityId,
                        v.CityName
                    FROM LegacyRequestCities AS lrc
                    INNER JOIN
                    (
                        VALUES
                            (101, 1, N'Cairo'),
                            (102, 1, N'Nasr City'),
                            (103, 1, N'Heliopolis'),
                            (104, 1, N'Maadi'),
                            (105, 1, N'New Cairo'),
                            (201, 2, N'Giza'),
                            (202, 2, N'Dokki'),
                            (203, 2, N'Mohandessin'),
                            (204, 2, N'6th of October'),
                            (205, 2, N'Sheikh Zayed'),
                            (301, 3, N'Alexandria'),
                            (302, 3, N'Smouha'),
                            (303, 3, N'Miami'),
                            (304, 3, N'Borg El Arab'),
                            (401, 4, N'Mansoura'),
                            (501, 5, N'Hurghada'),
                            (601, 6, N'Damanhur'),
                            (701, 7, N'Fayoum'),
                            (801, 8, N'Tanta'),
                            (901, 9, N'Ismailia'),
                            (1001, 10, N'Shibin El Kom'),
                            (1101, 11, N'Minya'),
                            (1201, 12, N'Benha'),
                            (1301, 13, N'Kharga'),
                            (1401, 14, N'Suez'),
                            (1501, 15, N'Aswan'),
                            (1601, 16, N'Assiut'),
                            (1701, 17, N'Beni Suef'),
                            (1801, 18, N'Port Said'),
                            (1901, 19, N'Damietta'),
                            (2001, 20, N'Zagazig'),
                            (2101, 21, N'Sharm El Sheikh'),
                            (2201, 22, N'Kafr El Sheikh'),
                            (2301, 23, N'Marsa Matrouh'),
                            (2401, 24, N'Luxor'),
                            (2501, 25, N'Qena'),
                            (2601, 26, N'Arish'),
                            (2701, 27, N'Sohag')
                    ) AS v(CityId, GovernorateId, CityName)
                        ON v.GovernorateId = lrc.GovernorateId
                        AND (
                            UPPER(v.CityName) = UPPER(lrc.LegacyEnglishName)
                            OR UPPER(v.CityName) = UPPER(lrc.LegacyCityName)
                        )
                )
                UPDATE cr
                SET
                    cr.CityId = rc.CityId,
                    cr.City = rc.CityName
                FROM ClientRequests AS cr
                INNER JOIN ResolvedCities AS rc
                    ON rc.RequestId = cr.Id;

                UPDATE cr
                SET cr.CityId = NULL
                FROM ClientRequests AS cr
                WHERE cr.CityId IS NOT NULL
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM Cities AS c
                      WHERE c.Id = cr.CityId
                        AND UPPER(LTRIM(RTRIM(ISNULL(cr.City, N'')))) = UPPER(c.Name)
                  );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_Cities_CityId",
                table: "ClientRequests",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
