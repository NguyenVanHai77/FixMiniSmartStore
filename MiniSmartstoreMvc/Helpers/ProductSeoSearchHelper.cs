using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MiniSmartstoreMvc.Models;

namespace MiniSmartstoreMvc.Helpers
{
    public static class ProductSeoSearchHelper
    {
        // ===== LƯU Ý: TỪ KHÓA MẶC ĐỊNH THEO DANH MỤC =====
        private static readonly Dictionary<string, string[]>
            DefaultCategoryKeywords =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ["laptop"] = new[]
                    {
                        "laptop",
                        "notebook",
                        "ultrabook",
                        "máy tính xách tay",
                        "máy tính cá nhân",
                        "portable computer"
                    },

                    ["dien thoai"] = new[]
                    {
                        "điện thoại",
                        "smartphone",
                        "smart phone",
                        "phone",
                        "mobile",
                        "cellphone",
                        "điện thoại thông minh"
                    },

                    ["tablet"] = new[]
                    {
                        "tablet",
                        "máy tính bảng",
                        "ipad",
                        "tab"
                    },

                    ["may tinh bang"] = new[]
                    {
                        "máy tính bảng",
                        "tablet",
                        "ipad",
                        "tab"
                    },

                    ["tai nghe"] = new[]
                    {
                        "tai nghe",
                        "headphone",
                        "headphones",
                        "headset",
                        "earphone",
                        "earphones",
                        "earbuds",
                        "earbud"
                    },

                    ["chuot"] = new[]
                    {
                        "chuột",
                        "mouse",
                        "gaming mouse",
                        "chuột máy tính",
                        "chuột gaming"
                    },

                    ["ban phim"] = new[]
                    {
                        "bàn phím",
                        "keyboard",
                        "gaming keyboard",
                        "mechanical keyboard",
                        "bàn phím cơ",
                        "bàn phím gaming"
                    },

                    ["man hinh"] = new[]
                    {
                        "màn hình",
                        "monitor",
                        "display",
                        "screen",
                        "màn hình máy tính"
                    },

                    ["dong ho thong minh"] = new[]
                    {
                        "đồng hồ thông minh",
                        "smartwatch",
                        "smart watch",
                        "wearable",
                        "đồng hồ"
                    },

                    ["smartwatch"] = new[]
                    {
                        "smartwatch",
                        "smart watch",
                        "đồng hồ thông minh",
                        "wearable"
                    },

                    ["loa"] = new[]
                    {
                        "loa",
                        "speaker",
                        "bluetooth speaker",
                        "loa bluetooth",
                        "wireless speaker"
                    },

                    ["camera"] = new[]
                    {
                        "camera",
                        "máy ảnh",
                        "webcam",
                        "camera kỹ thuật số"
                    },

                    ["webcam"] = new[]
                    {
                        "webcam",
                        "camera máy tính",
                        "camera",
                        "web camera"
                    },

                    ["sac"] = new[]
                    {
                        "sạc",
                        "charger",
                        "củ sạc",
                        "adapter",
                        "adapter sạc",
                        "bộ sạc"
                    },

                    ["cu sac"] = new[]
                    {
                        "củ sạc",
                        "charger",
                        "adapter",
                        "sạc",
                        "bộ sạc"
                    },

                    ["cap sac"] = new[]
                    {
                        "cáp sạc",
                        "cable",
                        "charging cable",
                        "cáp",
                        "dây sạc",
                        "usb cable"
                    },

                    ["o cung"] = new[]
                    {
                        "ổ cứng",
                        "hard drive",
                        "storage",
                        "ssd",
                        "hdd",
                        "ổ lưu trữ"
                    },

                    ["ssd"] = new[]
                    {
                        "ssd",
                        "solid state drive",
                        "ổ cứng ssd",
                        "ổ lưu trữ",
                        "storage"
                    },

                    ["hdd"] = new[]
                    {
                        "hdd",
                        "hard disk drive",
                        "hard drive",
                        "ổ cứng hdd",
                        "ổ cứng"
                    },

                    ["ram"] = new[]
                    {
                        "ram",
                        "memory",
                        "bộ nhớ",
                        "bộ nhớ ram"
                    },

                    ["cpu"] = new[]
                    {
                        "cpu",
                        "processor",
                        "vi xử lý",
                        "bộ xử lý"
                    },

                    ["card do hoa"] = new[]
                    {
                        "card đồ họa",
                        "graphics card",
                        "gpu",
                        "vga"
                    },

                    ["gpu"] = new[]
                    {
                        "gpu",
                        "graphics card",
                        "card đồ họa",
                        "vga"
                    },

                    ["phu kien"] = new[]
                    {
                        "phụ kiện",
                        "accessory",
                        "accessories",
                        "phụ kiện công nghệ"
                    },

                    ["pin du phong"] = new[]
                    {
                        "pin dự phòng",
                        "power bank",
                        "powerbank",
                        "sạc dự phòng"
                    },

                    ["op lung"] = new[]
                    {
                        "ốp lưng",
                        "case",
                        "phone case",
                        "ốp điện thoại"
                    }
                };
        // ===== KẾT THÚC TỪ KHÓA MẶC ĐỊNH THEO DANH MỤC =====


        // ===== LƯU Ý: TỰ TẠO SEO CHO SẢN PHẨM NẾU ĐANG TRỐNG =====
        public static void FillMissingSeo(
            Product product,
            Category category)
        {
            if (product == null ||
                category == null)
            {
                return;
            }


            if (string.IsNullOrWhiteSpace(
                    product.Alias))
            {
                product.Alias =
                    Truncate(
                        CreateAlias(product.Name),
                        200);
            }


            if (string.IsNullOrWhiteSpace(
                    product.SeoTitle))
            {
                product.SeoTitle =
                    Truncate(
                        $"{product.Name} | MiniSmartstore",
                        200);
            }


            if (string.IsNullOrWhiteSpace(
                    product.SeoDescription))
            {
                product.SeoDescription =
                    Truncate(
                        $"Xem {product.Name} thuộc danh mục " +
                        $"{category.Name} tại MiniSmartstore. " +
                        "Thông tin sản phẩm, giá bán, " +
                        "tình trạng hàng và các thông tin liên quan.",
                        500);
            }


            if (string.IsNullOrWhiteSpace(
                    product.SeoKeywords))
            {
                var keywords =
                    new List<string>();


                AddKeyword(
                    keywords,
                    product.Name);


                AddKeyword(
                    keywords,
                    product.ProductCode);


                AddKeyword(
                    keywords,
                    category.Name);


                AddKeyword(
                    keywords,
                    category.Alias);


                foreach (var keyword
                         in GetCategoryKeywords(category))
                {
                    AddKeyword(
                        keywords,
                        keyword);
                }


                product.SeoKeywords =
                    BuildKeywordString(
                        keywords,
                        500);
            }
        }
        // ===== KẾT THÚC TỰ TẠO SEO CHO SẢN PHẨM NẾU ĐANG TRỐNG =====


        // ===== LƯU Ý: LẤY TẤT CẢ TỪ KHÓA LIÊN QUAN ĐẾN DANH MỤC =====
        public static List<string>
            GetCategoryKeywords(
                Category category)
        {
            var result =
                new List<string>();


            if (category == null)
            {
                return result;
            }


            AddKeyword(
                result,
                category.Name);


            AddKeyword(
                result,
                category.Alias);


            AddKeywordsFromText(
                result,
                category.MetaKeywords);


            AddDefaultKeywords(
                result,
                category.Name);


            AddDefaultKeywords(
                result,
                category.Alias);


            if (category.ParentCategory != null)
            {
                AddKeyword(
                    result,
                    category.ParentCategory.Name);


                AddKeyword(
                    result,
                    category.ParentCategory.Alias);


                AddKeywordsFromText(
                    result,
                    category.ParentCategory.MetaKeywords);


                AddDefaultKeywords(
                    result,
                    category.ParentCategory.Name);


                AddDefaultKeywords(
                    result,
                    category.ParentCategory.Alias);
            }


            return result;
        }
        // ===== KẾT THÚC LẤY TẤT CẢ TỪ KHÓA LIÊN QUAN ĐẾN DANH MỤC =====


        // ===== LƯU Ý: KIỂM TRA TỪ KHÓA CÓ THUỘC NHÓM DANH MỤC KHÔNG =====
        public static bool IsCategoryRelated(
            string? search,
            Category? category)
        {
            if (category == null ||
                string.IsNullOrWhiteSpace(search))
            {
                return false;
            }


            var normalizedSearch =
                NormalizeForCompare(search);


            if (string.IsNullOrWhiteSpace(
                    normalizedSearch))
            {
                return false;
            }


            var categoryKeywords =
                GetCategoryKeywords(category);


            foreach (var categoryKeyword
                     in categoryKeywords)
            {
                var normalizedKeyword =
                    NormalizeForCompare(
                        categoryKeyword);


                if (string.IsNullOrWhiteSpace(
                        normalizedKeyword))
                {
                    continue;
                }


                // Ưu tiên chính xác:
                // smartphone = smartphone
                // notebook = notebook
                if (normalizedSearch.Equals(
                        normalizedKeyword,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }


                // Cho phép cụm dài chứa nhau.
                // Ví dụ:
                // "laptop gaming" chứa "laptop"
                // "tai nghe bluetooth" chứa "tai nghe"
                if (normalizedSearch.Length >= 5 &&
                    normalizedKeyword.Length >= 5)
                {
                    if (ContainsWholePhrase(
                            normalizedSearch,
                            normalizedKeyword) ||
                        ContainsWholePhrase(
                            normalizedKeyword,
                            normalizedSearch))
                    {
                        return true;
                    }
                }
            }


            return false;
        }
        // ===== KẾT THÚC KIỂM TRA TỪ KHÓA CÓ THUỘC NHÓM DANH MỤC KHÔNG =====


        // ===== LƯU Ý: TÌM CÁC DANH MỤC LIÊN QUAN TỪ TỪ KHÓA =====
        public static HashSet<int>
            GetRelatedCategoryIds(
                string? search,
                IReadOnlyCollection<Category> categories)
        {
            var result =
                new HashSet<int>();


            if (string.IsNullOrWhiteSpace(search) ||
                categories == null ||
                categories.Count == 0)
            {
                return result;
            }


            foreach (var category
                     in categories)
            {
                if (!IsCategoryRelated(
                        search,
                        category))
                {
                    continue;
                }


                AddCategoryAndChildren(
                    category.Id,
                    categories,
                    result);
            }


            return result;
        }
        // ===== KẾT THÚC TÌM CÁC DANH MỤC LIÊN QUAN TỪ TỪ KHÓA =====


        // ===== LƯU Ý: CHUẨN HÓA CHUỖI ĐỂ SO SÁNH TÌM KIẾM =====
        public static string NormalizeForCompare(
            string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            text = text
                .Trim()
                .ToLowerInvariant()
                .Replace('đ', 'd');


            string decomposed =
                text.Normalize(
                    NormalizationForm.FormD);


            var builder =
                new StringBuilder();


            foreach (char character
                     in decomposed)
            {
                UnicodeCategory category =
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            character);


                if (category !=
                    UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }


            string result =
                builder
                    .ToString()
                    .Normalize(
                        NormalizationForm.FormC);


            result =
                Regex.Replace(
                    result,
                    @"[^a-z0-9]+",
                    " ");


            result =
                Regex.Replace(
                    result,
                    @"\s+",
                    " ");


            return result.Trim();
        }
        // ===== KẾT THÚC CHUẨN HÓA CHUỖI ĐỂ SO SÁNH TÌM KIẾM =====


        // ===== LƯU Ý: THÊM TỪ KHÓA MẶC ĐỊNH THEO TÊN DANH MỤC =====
        private static void AddDefaultKeywords(
            List<string> result,
            string? categoryName)
        {
            var normalizedCategoryName =
                NormalizeForCompare(
                    categoryName);


            if (string.IsNullOrWhiteSpace(
                    normalizedCategoryName))
            {
                return;
            }


            foreach (var keywordGroup
                     in DefaultCategoryKeywords)
            {
                var normalizedGroupName =
                    NormalizeForCompare(
                        keywordGroup.Key);


                bool isMatch =
                    normalizedCategoryName.Equals(
                        normalizedGroupName,
                        StringComparison.OrdinalIgnoreCase);


                if (!isMatch &&
                    normalizedCategoryName.Length >= 4 &&
                    normalizedGroupName.Length >= 4)
                {
                    isMatch =
                        ContainsWholePhrase(
                            normalizedCategoryName,
                            normalizedGroupName) ||

                        ContainsWholePhrase(
                            normalizedGroupName,
                            normalizedCategoryName);
                }


                if (!isMatch)
                {
                    continue;
                }


                foreach (var keyword
                         in keywordGroup.Value)
                {
                    AddKeyword(
                        result,
                        keyword);
                }
            }
        }
        // ===== KẾT THÚC THÊM TỪ KHÓA MẶC ĐỊNH THEO TÊN DANH MỤC =====


        // ===== LƯU Ý: TÁCH META KEYWORDS CỦA DANH MỤC =====
        private static void AddKeywordsFromText(
            List<string> result,
            string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }


            string[] values =
                text.Split(
                    new[]
                    {
                        ',',
                        ';',
                        '|',
                        '\n',
                        '\r'
                    },
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);


            foreach (var value
                     in values)
            {
                AddKeyword(
                    result,
                    value);
            }
        }
        // ===== KẾT THÚC TÁCH META KEYWORDS CỦA DANH MỤC =====


        private static void AddKeyword(
            List<string> result,
            string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }


            keyword =
                Regex.Replace(
                    keyword.Trim(),
                    @"\s+",
                    " ");


            bool alreadyExists =
                result.Any(existing =>
                    existing.Equals(
                        keyword,
                        StringComparison.OrdinalIgnoreCase));


            if (alreadyExists)
            {
                return;
            }


            result.Add(keyword);
        }


        private static string BuildKeywordString(
            IEnumerable<string> keywords,
            int maxLength)
        {
            var builder =
                new StringBuilder();


            foreach (var keyword
                     in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }


                string nextValue =
                    builder.Length == 0
                        ? keyword
                        : ", " + keyword;


                if (builder.Length +
                    nextValue.Length >
                    maxLength)
                {
                    continue;
                }


                builder.Append(nextValue);
            }


            return builder.ToString();
        }


        private static void AddCategoryAndChildren(
            int categoryId,
            IReadOnlyCollection<Category> categories,
            HashSet<int> result)
        {
            if (!result.Add(categoryId))
            {
                return;
            }


            var childCategoryIds =
                categories
                    .Where(c =>
                        c.ParentCategoryId ==
                        categoryId)
                    .Select(c => c.Id)
                    .ToList();


            foreach (var childCategoryId
                     in childCategoryIds)
            {
                AddCategoryAndChildren(
                    childCategoryId,
                    categories,
                    result);
            }
        }


        private static bool ContainsWholePhrase(
            string source,
            string value)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(value))
            {
                return false;
            }


            if (source.Equals(
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }


            string paddedSource =
                $" {source.Trim()} ";


            string paddedValue =
                $" {value.Trim()} ";


            return paddedSource.Contains(
                paddedValue,
                StringComparison.OrdinalIgnoreCase);
        }


        private static string CreateAlias(
            string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            text = text
                .Trim()
                .ToLowerInvariant()
                .Replace('đ', 'd');


            string decomposed =
                text.Normalize(
                    NormalizationForm.FormD);


            var builder =
                new StringBuilder();


            foreach (char character
                     in decomposed)
            {
                UnicodeCategory category =
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            character);


                if (category !=
                    UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }


            string alias =
                builder
                    .ToString()
                    .Normalize(
                        NormalizationForm.FormC);


            alias =
                Regex.Replace(
                    alias,
                    @"[^a-z0-9\s-]",
                    "");


            alias =
                Regex.Replace(
                    alias,
                    @"\s+",
                    "-");


            alias =
                Regex.Replace(
                    alias,
                    @"-+",
                    "-");


            return alias.Trim('-');
        }


        private static string Truncate(
            string? value,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }


            value = value.Trim();


            if (value.Length <= maxLength)
            {
                return value;
            }


            return value
                .Substring(0, maxLength)
                .Trim();
        }
    }
}