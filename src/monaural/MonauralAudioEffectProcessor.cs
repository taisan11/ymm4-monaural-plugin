using YukkuriMovieMaker.Player.Audio;
using YukkuriMovieMaker.Player.Audio.Effects;

namespace YMM4SamplePlugin.AudioEffect
{
    internal class MonauralAudioEffectProcessor : AudioEffectProcessorBase
    {
        readonly MonauralAudio item;
        readonly TimeSpan duration;

        //出力サンプリングレート。リサンプリング処理をしない場合はInputのHzをそのまま返す。
        public override int Hz => Input?.Hz ?? 0;

        //出力するサンプル数
        public override long Duration => (long)(duration.TotalSeconds * Input?.Hz ?? 0) * 2;

        public MonauralAudioEffectProcessor(MonauralAudio item, TimeSpan duration)
        {
            this.item = item;
            this.duration = duration;
        }

        //シーク処理
        protected override void seek(long position)
        {
            Input?.Seek(position);
        }

        //エフェクトを適用する
        protected override int read(float[] destBuffer, int offset, int count)
        {
            Input?.Read(destBuffer, offset, count);
            
            // モノラル化の強度に応じてステレオをモノラルに変換
            for (int i = 0; i < count; i += 2)
            {
                // 強度を取得 (0-100% → 0.0-1.0)
                var intensity = (float)(item.Intensity.GetValue((long)((Position + i) / 2.0), (long)(Duration / 2.0), Hz) / 100.0);
                
                // 左右チャンネルの値を取得
                float left = destBuffer[offset + i + 0];
                float right = destBuffer[offset + i + 1];
                
                // モノラル値（左右の平均）を計算
                float mono = (left + right) * 0.5f;
                
                // 強度に応じて元の音とモノラル音をミックス
                // intensity = 0: 完全にステレオ, intensity = 1: 完全にモノラル
                destBuffer[offset + i + 0] = left * (1.0f - intensity) + mono * intensity;
                destBuffer[offset + i + 1] = right * (1.0f - intensity) + mono * intensity;
            }
            return count;
        }
    }
}