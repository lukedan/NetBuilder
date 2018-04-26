from NetBuilder import *

def RegisterAll(register):
	def simpleFormatter(strVal):
		def format(list):
			print list
			return strVal.format(*list)
		return format

	anyType, _ = register.Type('any', ConstantType.None)
	intType, intTypeNode = register.Type('int', ConstantType.String)
	floatType, floatTypeNode = register.Type('float', ConstantType.String)
	tensorType, _ = register.Type('tensor', ConstantType.None)
	listType, listTypeNode = register.Type('list', ConstantType.String)
	typeType, typeTypeNode = register.Type('type', ConstantType.Enum, 'uint8', 'int32', 'float', 'double')

	inputNode = register.Node(
		'Input',
		[Variable(listType, 'dimensions'), Variable(typeType, 'type')],
		[Variable(tensorType, 'value')])

	fullyConnectedNode = register.Node(
		'FullyConnected',
		[Variable(tensorType, 'input'), Variable(listType, 'outputSize')],
		[Variable(tensorType, 'output')])
	conv2dNode = register.Node(
		'Conv2d',
		[Variable(tensorType, 'input'), Variable(listType, 'kernelSize')],
		[Variable(tensorType, 'output')])

	sigmoidNode = register.Node('Sigmoid', [Variable(tensorType, 'input')], [Variable(tensorType, 'output')])
	tanhNode = register.Node('Tanh', [Variable(tensorType, 'input')], [Variable(tensorType, 'output')])
	reLuNode = register.Node('ReLu', [Variable(tensorType, 'input')], [Variable(tensorType, 'output')])

	castTensorNode = register.Node(
		'CastTensor',
		[Variable(tensorType, 'input'), Variable(typeType, 'targetType')],
		[Variable(tensorType, 'output')])

	resultNode = register.Node('Result', [Variable(tensorType, 'result')], [])

	# TODO correct code
	register.Generator('tensorflow-py', 'None', {
		intTypeNode: simpleFormatter('{0} = {1}\n'),
		floatTypeNode: simpleFormatter('{0} = {1}\n'),
		listTypeNode: simpleFormatter('{0} = [{1}]\n'),
		typeTypeNode: simpleFormatter('{0} = {1}\n'),
		inputNode: simpleFormatter('{2} = tf.placeholder({0}, dtype = {1})\n'),
		fullyConnectedNode: simpleFormatter('{2} = tf.fullyconnected({0}, {1})\n'),
		conv2dNode: simpleFormatter('{2} = tf.nn.conf2d({0}, {1})\n'),
		sigmoidNode: simpleFormatter('{1} = tf.sigmoid({0})\n'),
		tanhNode: simpleFormatter('{1} = tf.tanh({0})\n'),
		reLuNode: simpleFormatter('{1} = tf.relu({0})\n'),
		castTensorNode: simpleFormatter('{2} = tf.cast({0}, {1})\n'),
		resultNode: simpleFormatter('{0}\n')
	})
