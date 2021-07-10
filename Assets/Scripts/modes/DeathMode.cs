using UnityEngine;
namespace unitrys{
    public class DeathMode : Mode
    {
        public const string id = "T.A Death";
        public const string TETRION_COLOR = "#FF0000";
        public DeathMode() : base(){
            _randomizer = new TGMRandomizer();
            _rotationSystem = new ARS();
            _maxLevel = 999;
        }

        protected override void SetTimings()
        {
            _timings.Add(new int[2] {0, 99}, new int[6] {16,12,10,30,12,1});
            _timings.Add(new int[2] {100, 199}, new int[6] {12,6,10,26,6,1});
            _timings.Add(new int[2] {200, 299}, new int[6] {12,6,9,22,6,1});
            _timings.Add(new int[2] {300, 399}, new int[6] {6,6,8,18,6,1});
            _timings.Add(new int[2] {400, 499}, new int[6] {5,5,6,15,5,1});
            _timings.Add(new int[2] {500, 999}, new int[6] {4,4,6,15,4,1});
            _gravities.Add(0, 5120); // 20G
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