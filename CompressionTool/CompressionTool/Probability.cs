using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionTool
{
    class Probability
    {
        private string m_Text;
        private int m_SymbolCount;
        private Dictionary<char, int> m_CharactersDictionary;
        private Dictionary<char, double> m_CharactersProbability;

        private void CountCharacters()
        {
            for(int i = 0; i < m_Text.Length; i++)
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

            m_SymbolCount = m_CharactersDictionary.Count;
        }

        private void CalculateProbability()
        {
            foreach (KeyValuePair<char, int> entry in m_CharactersDictionary)
            {
                double probability = (double)entry.Value / (double)m_Text.Length;

                m_CharactersProbability.Add(entry.Key, probability);
            }
        }

        public Probability(string Text)
        {
            m_Text = Text;
            m_CharactersDictionary = new Dictionary<char, int>();
            m_CharactersProbability = new Dictionary<char, double>();
        }

        public Dictionary<char,int> GetCharactersCount()
        {
            CountCharacters();

            return m_CharactersDictionary;
        }

        public Dictionary<char, double> GetCharactersProbability()
        {
            CountCharacters();

            CalculateProbability();

            return m_CharactersProbability;
        }
    }
}
