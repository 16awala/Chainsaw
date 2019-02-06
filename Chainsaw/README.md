# Chainsaw
###### An exercise in overengineering a very niche use case

## **DISCLAIMER:**
Chainsaw was developed entirely as a personal exercise in programming. It was made with little experience and was not intended to be anything more than a novelty.
I make no claims as to the stability, or security of Chainsaw, and am not responsible for whatever is done with it.
##
### **What does chainsaw do?**
Chainsaw is an algorithm that ingests expressions in the form of standard math notation to be evaluated as quickly as possible.
This is done by performing just-in-time order-of-operations analysis on a parsed expression before the first evaluation, and using the results of that analysis for all future evaluations.
By only analyzing how to evaluate a given expression once, a very large amount of data can be piped through it with reduced cpu time.
This allows expressions defined on the fly in a readable format to acheive high performance.
The evaluation algorithm can also be run in parallel to take advantage of multiple threads.

### **Why "Chainsaw" ?**
I could come up with any number of comparisons for the algorithm to a chainsaw to justify the name, but the reality is when I was creating the repo Github recommended "turbo-chainsaw". I thought Chainsaw sounded neat so I went with that.

### **What is Pull-Start?**
Pull-Start is a wrapper that allows Chainsaw to be run in a command line environment from a windows executable (.exe).
Pull-Start provides functionality such as defining, debugging, and evaluating expressions.
