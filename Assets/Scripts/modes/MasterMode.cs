/*
Based on https://tetris.fandom.com/wiki/Tetris_The_Absolute_The_Grand_Master_2
*/
using UnityEngine;
namespace unitrys{
    public class MasterMode : Mode
    {
        public const string id = "Master";
        public const string TETRION_COLOR = "#0B43FF";
        public MasterMode() : base(){
            _randomizer = new TGMRandomizer();
            _rotationSystem = new ARS();
            _maxLevel = 999;
        }

        protected override void SetTimings()
        {
            _timings.Add(new int[2] {0, 499}, new int[5] {25,0,16,30,40});
            _timings.Add(new int[2] {500, 599}, new int[5] {25,0,10,30,25});
            _timings.Add(new int[2] {600, 699}, new int[5] {25,0,10,30,16});
            _timings.Add(new int[2] {700, 799}, new int[5] {16,0,10,30,12});
            _timings.Add(new int[2] {800, 899}, new int[5] {12,0,16,30,40});
            _timings.Add(new int[2] {900, 999}, new int[5] {12,0,16,30,40});

            _gravities.Add(0, 4);
            _gravities.Add(30, 6);
            _gravities.Add(35, 8);
            _gravities.Add(40, 10);
            _gravities.Add(50, 12);
            _gravities.Add(60, 16);
            _gravities.Add(70, 32);
            _gravities.Add(80, 48);
            _gravities.Add(90, 64);
            _gravities.Add(100, 80);
            _gravities.Add(120, 96);
            _gravities.Add(140, 112);
            _gravities.Add(160, 128);
            _gravities.Add(170, 144);
            _gravities.Add(200, 4);
            _gravities.Add(220, 32);
            _gravities.Add(230, 64);
            _gravities.Add(233, 96);
            _gravities.Add(236, 128);
            _gravities.Add(239, 160);
            _gravities.Add(243, 192);
            _gravities.Add(247, 224);
            _gravities.Add(251, 256); // 1G
            _gravities.Add(300, 512); // 2G
            _gravities.Add(330, 768); // 3G
            _gravities.Add(360, 1024); // 4G
            _gravities.Add(400, 1280); // 5G
            _gravities.Add(420, 1024); // 4G
            _gravities.Add(450, 768); // 3G
            _gravities.Add(500, 5120); // 20G
        }

        protected override Color GetTetrionColor()
        {
            Color color;
            ColorUtility.TryParseHtmlString(TETRION_COLOR, out color);
            return color;
        }

        public override string GetId()
        {
            return id;
        }
    }
}