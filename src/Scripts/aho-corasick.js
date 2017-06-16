/* Created by Kevin Bui (BuiDangKhanh1111@gmail.com)
 * Created on 2017-06-13

 aho-corasick.js

 Multiple string searching by Aho-Corasick algorithm

*/

"use strict";
//===================== Node =====================//
function Node(char) {
    this.edges = {};
    this.keyword = undefined;
    this.is_leaf = true;

    this.failure_node = undefined;
    this.suffix_key = undefined;
}
Node.prototype = {
    constructor: Node,

    add_edge: function (char) {
        var node = this.edges[char];
        if (!node) {
            node = this.edges[char] = new Node(char);
            this.is_leaf = false;
        }

        return node;
    }
}

//===================== AhoCorasick main class =====================//
function AhoCorasick(ignore_case, words) {
    this.root = new Node();
    this.ignore_case = ignore_case;

    this.build_matching_machine(words);
}
AhoCorasick.prototype = {
    constructor: AhoCorasick,

    build_matching_machine: function (words) {
        //Contruct Trie
        words.forEach(function (keyword) {
            if (this.ignore_case) keyword = keyword.toLowerCase();

            var currentState = this.root;

            for (var i = 0, c = ''; c = keyword.charAt(i); ++i) {
                currentState = currentState.add_edge(c);
            };

            currentState.keyword = keyword;
            currentState.suffix_key = { value: currentState };
        }, this);

        //Construct Finite-State Machine FSM, by Breadth-First Search
        var queue = [];
        queue.push(this.root);

        while (queue.length > 0) {
            var state = queue.shift();
            var failureState = state.failure_node;

            // build Failure edges for each child node in current state
            for (var c in state.edges) {
                var child = state.edges[c];

                while (failureState && !failureState.edges[c])
                    failureState = failureState.failure_node;

                if (!failureState)
                    child.failure_node = this.root;
                else {
                    child.failure_node = failureState.edges[c];
                    if (child.failure_node.suffix_key) {
                        if (!child.suffix_key)
                            child.suffix_key = child.failure_node.suffix_key;
                        else
                            child.suffix_key.next = child.failure_node.suffix_key;
                    }
                }

                queue.push(child);
            }
        }
    },

    search: function (text) {
        if (this.ignore_case) text = text.toLowerCase();

        var results = [];
        var currentState = this.root;

        for (var index = 0, c = ''; c = text.charAt(index); ++index)
        {
            while (currentState && !currentState.edges[c])
                currentState = currentState.failure_node;

            if (!currentState)
                currentState = this.root;
            else
            {
                currentState = currentState.edges[c];
                // found match, add match to basket
                this.add_match_to_result(currentState, index, results);

                if (currentState.is_leaf)
                    currentState = currentState.failure_node;
            }
        }

        return results;
    },

    add_match_to_result: function(currentState, index, results) {
        var suffix = currentState.suffix_key;
        while (suffix)
        {
            results.push(
            {
                keyword: suffix.value.keyword,
                position: index - suffix.value.keyword.length + 1
            });

            suffix = suffix.next;
        }
    },

    filter_match: function(matches) {
        matches.sort(function(x, y) {
            var ret = x.position - y.position;
            if (ret == 0)
                ret = y.keyword.length - x.keyword.length;
            return ret;
        });

        var res = [];
        var position = 0;

        for (var i = 0, m = ''; m = matches[i]; ++i)
        {
            if (position <= m.position)
            {
                res.push(m);
                position = m.position + m.keyword.length;
            }
        }
        return res;
    }
}

//===================== Using =====================//
function test_example_AhoCorasick() {
    var ac = new AhoCorasick(true, ["he", "sherd", "herdsman", "she", "MAn"]);
    var res = ac.search("sheRdsmane")
    res.forEach(function (m) {
        console.log(m);
    });

    console.log('----------');

    res = ac.filter_match(res);
    res.forEach(function (m) {
        console.log(m);
    });
}
//call testExample(); to see result in console windows of browser;