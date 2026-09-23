using UnityEngine;

namespace BS.Render
{
    public class RenderController
    {
        SpriteParams _spriteParams;
        RenderParams _renderParams;
        Matrix4x4 _instData;

        public RenderController()
        {
            _spriteParams = default;
            _renderParams = default;
            _instData = Matrix4x4.identity;
        }

        public RenderController SetSprite(Sprite sprite)
        {
            _spriteParams = new SpriteParams(sprite, _spriteParams.color);
            return this;
        }

        public RenderController SetColor(Color color)
        {
            _spriteParams = new SpriteParams(_spriteParams.sprite, color);
            return this;
        }

        public RenderController SetRender(RenderParams renderParams)
        {
            _renderParams = renderParams;
            return this;
        }

        public RenderController SetPosition(Vector2 position)
        {
            _instData = Matrix4x4.TRS(position, _instData.rotation, _instData.lossyScale);
            return this;
        }

        public RenderController SetRotation(float rotation)
        {
            _instData = Matrix4x4.TRS(_instData.GetPosition(), Quaternion.Euler(0, 0, rotation), _instData.lossyScale);
            return this;
        }

        public RenderController SetScale(Vector3 scale)
        {
            _instData = Matrix4x4.TRS(_instData.GetPosition(), _instData.rotation, scale);
            return this;
        }

        public RenderController SetMatrix(Matrix4x4 matrix)
        {
            _instData = matrix;
            return this;
        }

        public void Render()
        {
            if (_spriteParams.sprite == null || _renderParams.material == null)
                return;

            Graphics.RenderSprite(_renderParams, _spriteParams, 0, _instData);
        }
    }
}