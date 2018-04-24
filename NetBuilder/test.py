from NetBuilder import *

intType = register.Type('int')
floatType = register.Type('float')
tensorType = register.Type('tensor')
listType = register.Type('list')

# placeholders
# each type will have their own constant editors
register.Node('Size (Placeholder)', [], [Variable(listType, 'output')])

register.Node('Input', [], [Variable(tensorType, 'value')])
register.Node(
	'FullyConnected',
	[Variable(tensorType, 'input'), Variable(listType, 'outputSize')],
	[Variable(tensorType, 'output')])
register.Node(
	'Conv2d',
	[Variable(tensorType, 'input'), Variable(listType, 'kernelSize')],
	[Variable(tensorType, 'output')])
register.Node('ReLu', [Variable(tensorType, 'input')], [Variable(tensorType, 'output')])
register.Node('Sigmoid', [Variable(tensorType, 'input')], [Variable(tensorType, 'output')])
register.Node('Result', [Variable(tensorType, 'result')], [])
