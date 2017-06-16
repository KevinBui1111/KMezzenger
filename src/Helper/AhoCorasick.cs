/* Created by Kevin Bui (BuiDangKhanh1111@gmail.com)
 * Created on 2017-06-13
*/
using System;
using System.Collections;
using System.Collections.Generic;

namespace KevinHelper
{
    class AhoCorasick
    {
        Node root;
        bool ignore_case = false;

        public AhoCorasick(bool ignore_case, params string[] keywords)
        {
            this.ignore_case = ignore_case;
            if (ignore_case)
                for (int i = 0; i < keywords.Length; ++i)
                    keywords[i] = keywords[i].ToLower();

            BuildMatchingMachine(keywords);
        }

        void BuildMatchingMachine(string[] words)
        {
            #region Contruct Trie

            root = new Node();

            foreach (string keyword in words)
            {
                Node currentState = root;

                foreach (char c in keyword)
                {
                    if (currentState[c] == null)
                        currentState.AddEdge(c);

                    currentState = currentState[c];
                }

                currentState.Key = keyword;
                currentState.SuffixKey = new LinkedNode<Node> { Value = currentState };
            }

            #endregion
            
            #region Construct Finite-State Machine FSM, by Breadth-First Search

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Node state = queue.Dequeue();
                Node failureState = state.Failure;

                // build Failure edges for each child node in current state
                foreach (Node child in state)
                {
                    while (failureState != null && failureState[child.Value] == null)
                        failureState = failureState.Failure;

                    if (failureState == null)
                        child.Failure = root;
                    else
                    {
                        child.Failure = failureState[child.Value];
                        if (child.Failure.SuffixKey != null)
                        {
                            if (child.SuffixKey == null)
                                child.SuffixKey = child.Failure.SuffixKey;
                            else
                                child.SuffixKey.Next = child.Failure.SuffixKey;
                        }
                    }

                    queue.Enqueue(child);
                }
            }

            #endregion
        }
        void add_match_to_result(Node currentState, int index, List<MatchResult> results)
        {
            LinkedNode<Node> suffix = currentState.SuffixKey;
            while (suffix != null)
            {
                results.Add(new MatchResult
                {
                    keyword = suffix.Value.Key,
                    position = index - suffix.Value.Key.Length + 1
                });

                suffix = suffix.Next;
            }
        }

        public List<MatchResult> Search(string text)
        {
            if (ignore_case) text = text.ToLower();

            List<MatchResult> results = new List<MatchResult>();
            Node currentState = root;

            for (int index = 0; index < text.Length; ++index)
            {
                char c = text[index];

                while (currentState != null && currentState[c] == null)
                    currentState = currentState.Failure;

                if (currentState == null)
                    currentState = root;
                else
                {
                    currentState = currentState[c];
                    // found match, add match to basket
                    add_match_to_result(currentState, index, results);

                    if (currentState.IsLeaf)
                        currentState = currentState.Failure;
                }
            }

            return results;
        }

        public List<MatchResult> FilterMatch(List<MatchResult> matches)
        {
            matches.Sort();

            List<MatchResult> res = new List<MatchResult>();
            int position = 0;

            foreach (MatchResult m in matches)
            {
                if (position <= m.position)
                {
                    res.Add(m);
                    position = m.position + m.keyword.Length;
                }
            }
            return res;
        }

        class Node : IEnumerable<Node>
        {
            const char min_char = (char)32, max_char = (char)126;
            Node[] edges; // 95 characters from 32-126
            List<Node> list_edge = new List<Node>();

            public Node()
            {
                edges = new Node[max_char - min_char + 1]; // 94 characters from 33-126
                IsLeaf = true;
            }

            public char Value { get; private set; }
            public bool IsLeaf { get; private set; }
            public string Key { get; set; }
            public Node Failure { get; set; }
            public LinkedNode<Node> SuffixKey { get; set; }

            public Node this[char c]
            {
                get
                {
                    return min_char <= c && c <= max_char ?
                        edges[c - min_char] :
                        null;
                }
            }
            public void AddEdge(char c)
            {
                Node edge = new Node { Value = c };
                edges[c - min_char] = edge;
                list_edge.Add(edge);
                IsLeaf = false;
            }

            IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
            {
                return list_edge.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
        class LinkedNode<T>
        {
            public T Value { get; set; }
            public LinkedNode<T> Next { get; set; }
        }
    }


    class MatchResult : IComparable<MatchResult>
    {
        public string keyword { get; set; }
        public int position { get; set; }

        int IComparable<MatchResult>.CompareTo(MatchResult other)
        {
            int ret = this.position.CompareTo(other.position);
            if (ret == 0)
                ret = other.keyword.Length.CompareTo(this.keyword.Length);
            return ret;
        }
    }
}
