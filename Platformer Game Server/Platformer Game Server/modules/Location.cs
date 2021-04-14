using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class Location {
        private float x, y;
        private float rY;

        public Location(float x, float y, float rY) {
            Set(x, y, rY);
        }

        public Location() { }

        public Location Set(float x, float y, float rY) {
            this.x = x;
            this.y = y;
            this.rY = rY;
            return this;
        }

        public float GetX() {
            return x;
        }

        public float GetY() {
            return y;
        }

        public float GetRY() {
            return rY;
        }

        public Location SetX(float x) {
            this.x = x;
            return this;
        }

        public Location SetY(float y) {
            this.y = y;
            return this;
        }

        public Location SetRY(float rY) {
            this.rY = rY;
            return this;
        }
    }
}
