using System.Collections.Generic;

namespace Huww98.FiveInARow.Engine.Tests
{
    class ForbiddenCheckData
    {
        public static IEnumerable<object[]> Data()
        {
            // Generally, it should be false
            yield return new object[] { new string []
            {
                "*"
            }, false};

            // 三三禁手
            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ ",
                "_ _ _ _ _ _ ",
                "_ o o * _ _ ",
                "_ _ o _ _ _ ",
                "_ o _ _ _ _ ",
                "_ _ _ _ _ _ ",
            }, true};

            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ _ ",
                "_ _ _ _ o _ _ ",
                "_ _ o * o _ _ ",
                "_ _ _ _ _ _ _ ",
                "_ o _ _ _ _ _ ",
                "_ _ _ _ _ _ _ ",
            }, true};

            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ ",
                "_ o o _ * _ ",
                "_ _ _ o _ _ ",
                "_ _ _ _ _ _ ",
                "_ o _ _ _ _ ",
                "_ _ _ _ _ _ ",
            }, true};

            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ ",
                "_ _ _ _ _ _ ",
                "x o o * _ _ ",
                "_ _ o _ _ _ ",
                "_ o _ _ _ _ ",
                "_ _ _ _ _ _ ",
            }, false};

            // 四四禁手
            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ ",
                "_ o o o * _ ",
                "_ _ _ o _ _ ",
                "_ _ o _ _ _ ",
                "_ o _ _ _ _ ",
                "_ _ _ _ _ _ ",
            }, true};

            yield return new object[] { new string []
            {
                "_ _ _ _ _ o ",
                "o o _ o * _ ",
                "_ _ _ o _ _ ",
                "_ _ o _ _ _ ",
                "_ _ _ _ _ _ ",
            }, true};

            yield return new object[] { new string []
            {
                "_ _ o _ _ _ _ ",
                "o o _ * o _ _ ",
                "_ _ _ _ _ _ _ ",
                "_ _ _ _ _ o _ ",
                "_ _ _ _ _ _ o ",
            }, true};

            yield return new object[] { new string []
            {
                "_ o _ _ _ _ ",
                "o o * o x _ ",
                "_ _ _ _ _ _ ",
                "_ _ _ _ o _ ",
                "_ _ _ _ _ o ",
            }, false};

            // 长连禁手
            yield return new object[] { new string []
            {
                "o o o * o o ",
            }, true};

            // 三四不是禁手
            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ _ ",
                "_ _ _ _ _ o _ ",
                "_ _ o o * _ _ ",
                "_ _ _ o _ _ _ ",
                "_ _ o _ _ _ _ ",
                "_ _ _ _ _ _ _ ",
                "_ _ _ _ _ _ _ ",
            }, false};

            // 至少有一个三在变成活四时一定会触发其他禁手，则这一步不是禁手
            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ _ _ _ _ _ ",
                "_ _ _ _ _ _ _ _ o _ _ ",
                "_ _ _ _ _ _ _ o _ _ _ ",
                "_ _ _ _ _ _ _ _ _ _ _ ",
                "_ _ _ o o * _ _ _ _ _ ",
                "o _ _ _ o _ _ _ _ _ _ ",
                "_ o _ o _ _ _ _ _ _ _ ",
                "_ _ _ _ _ _ _ _ _ _ _ ",
                "_ _ _ o _ _ _ _ _ _ _ ",
                "_ _ _ _ o _ _ _ _ _ _ ",
                "_ _ _ _ _ o _ _ _ _ _ ",
            }, false};

            yield return new object[] { new string []
            {
                "_ _ _ o _ _ _ _ ",
                "_ _ _ o _ _ _ _ ",
                "_ _ _ o _ _ _ _ ",
                "_ _ o _ o * _ _ ",
                "_ _ _ o o _ _ _ ",
                "_ _ _ o _ _ _ _ ",
                "_ _ _ _ _ _ _ _ ",
                "_ _ _ _ _ _ _ _ ",
            }, false};

            // 这个规定是递归的
            yield return new object[] { new string []
            {
                "_ _ _ o _ o _ _ _ _ _ ",
                "_ _ _ _ o _ _ _ _ _ _ ",
                "_ _ _ o _ _ _ _ _ o _ ",
                "_ _ _ o o * _ _ _ o _ ",
                "_ o _ _ o _ o o _ o _ ",
                "o _ _ o _ _ o _ o o _ ",
                "_ _ _ _ _ _ _ _ _ _ _ ",
                "_ _ _ _ _ _ _ _ _ o _ ",
            }, true};

            // 四四禁手可能出现在一条直线上
            yield return new object[] { new string []
            {
                "o _ o * o _ o ",
            }, true};

            yield return new object[] { new string []
            {
                "o o _ * o _ o o ",
            }, true};

            yield return new object[] { new string []
            {
                "o o o _ * _ o o o ",
            }, true};

            yield return new object[] { new string []
            {
                "_ o o o * _ ",
            }, false};

            // 如果这一步能赢则不是禁手
            yield return new object[] { new string []
            {
                "_ _ _ _ _ _ _ ",
                "_ _ _ _ o _ _ ",
                "_ _ o o * _ _ ",
                "_ _ _ o o _ _ ",
                "_ _ o _ o _ _ ",
                "_ _ _ _ o _ _ ",
                "_ _ _ _ _ _ _ ",
            }, false};
        }
    }
}