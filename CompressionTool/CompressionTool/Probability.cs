using System.Collections.Generic;

namespace CompressionTool
{
    class Probability
    {
        private List<byte> m_Text;
        private Dictionary<byte, int> m_CharactersDictionary;
       
        private void CountCharacters()
        {
            for(int i = 0; i < m_Text.Count; i++)
            {
                if (m_CharactersDictionary.ContainsKey(m_Text[i]))
                {
                    m_CharactersDictionary[m_Text[i]]++;
                }
                else
                {
                    m_CharactersDictionary.Add(m_Text[i], 1);
                }
            }
        }

        public Probability(List<byte> Text)
        {
            m_Text = Text;
            m_CharactersDictionary = new Dictionary<byte, int>();
        }

        public Dictionary<byte,int> GetCharactersCount()
        {
            CountCharacters();

            return m_CharactersDictionary;
        }
    }
}
